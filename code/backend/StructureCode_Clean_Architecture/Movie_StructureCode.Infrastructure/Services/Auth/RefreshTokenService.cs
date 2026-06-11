using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.RefreshAccessToken;
using Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Infrastructure.Services.Auth
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenProvider _tokenProvider;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RefreshTokenService(
            IRepository<RefreshToken> refreshTokenRepository,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            ITokenProvider tokenProvider,
            ITokenCacheService tokenCacheService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tokenProvider = tokenProvider;
            _tokenCacheService = tokenCacheService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public double GetAccessTokenExpirationMinutes()
        {
            var value = _configuration["JWT:ExpirationMinutes"];
            return double.TryParse(value, out var minutes) ? minutes : 60;
        }

        public double GetRefreshTokenExpirationDays()
        {
            var value = _configuration["JWT:RefreshTokenExpirationDays"];
            return double.TryParse(value, out var days) ? days : 7;
        }

        public async Task<bool> RevokeRefreshTokenAsync(RefreshToken token)
        {
            try
            {
                if (token == null)
                    return false;

                token.RevokeAndMarkUsed();
                _refreshTokenRepository.Update(token);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeAllRefreshTokensAsync(Guid userId)
        {
            try
            {
                var refreshTokens = await _refreshTokenRepository.FindAsync(
                    rt => rt.UserId == userId && !rt.IsRevoked,
                    CancellationToken.None);

                if (!refreshTokens.Any())
                    return true;

                foreach (var refreshToken in refreshTokens)
                {
                    refreshToken.Revoke();
                    _refreshTokenRepository.Update(refreshToken);
                }
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenByJtiAsync(string jti)
        {
            try
            {
                var refreshTokens = await _refreshTokenRepository.FindAsync(
                    rf => rf.Jti == jti,
                    CancellationToken.None);

                return refreshTokens.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private (string DeviceName, string DeviceType, string Ip) GetRequestDetails()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
            var ip = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var deviceType = userAgent.Contains("Mobi", StringComparison.OrdinalIgnoreCase) ? "mobile" : "desktop";
            return (userAgent, deviceType, ip);
        }

        public async Task<Result<RefreshAccessTokenResponse>> RefreshAccessTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            var principal = _tokenProvider.DecodeRefreshToken(refreshToken, out var securityToken);
            var jwtToken = securityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken;
            if (principal == null || jwtToken == null)
            {
                return Result.Failure<RefreshAccessTokenResponse>(
                    new Error("Auth.RefreshInvalid", "refresh_invalid"));
            }

            if (!jwtToken.TryGetClaim("token_type", out var tokenTypeClaim) ||
                tokenTypeClaim?.Value != "refresh")
            {
                return Result.Failure<RefreshAccessTokenResponse>(
                    new Error("Auth.RefreshInvalid", "refresh_invalid"));
            }

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return Result.Failure<RefreshAccessTokenResponse>(
                    new Error("Auth.SessionExpired", "session_expired"));
            }

            var old_refresh_jti = jwtToken.Id;
            if (string.IsNullOrEmpty(old_refresh_jti))
            {
                return Result.Failure<RefreshAccessTokenResponse>(
                    new Error("Auth.RefreshInvalid", "refresh_invalid"));
            }

            var userIdClaim = jwtToken.Subject;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Result.Failure<RefreshAccessTokenResponse>(
                    new Error("Auth.RefreshInvalid", "refresh_invalid"));
            }

            var userId = Guid.Parse(userIdClaim);

            // 2. Query Redis for old session
            string? old_accessJti = null;
            try
            {
                old_accessJti = await _tokenCacheService.GetRefreshTokenSessionAsync(old_refresh_jti);
            }
            catch (Exception)
            {
                // Redis offline/failure, trigger DB fallback
            }

            if (old_accessJti == null)
            {
                // Redis Miss / Failure: Fallback DB
                var dbToken = await GetRefreshTokenByJtiAsync(old_refresh_jti);
                if (dbToken == null || dbToken.ExpiredAt < DateTime.UtcNow)
                {
                    return Result.Failure<RefreshAccessTokenResponse>(
                        new Error("Auth.SessionExpired", "session_expired"));
                }

                if (dbToken.IsRevoked)
                {
                    return Result.Failure<RefreshAccessTokenResponse>(
                        new Error("Auth.TokenRevoked", "token_revoked"));
                }

                if (dbToken.IsUsed)
                {
                    // REUSE ATTACK detected: Revoke all user sessions
                    try
                    {
                        await _tokenCacheService.RemoveAllUserSessionsAsync(userId.ToString());
                    }
                    catch (Exception) { }
                    await RevokeAllRefreshTokensAsync(userId);

                    return Result.Failure<RefreshAccessTokenResponse>(
                        new Error("Auth.TokenReused", "token_reused"));
                }

                // Restore Redis (Warm up/Assign back when Redis is restarted)
                old_accessJti = dbToken.AccessJti;
                try
                {
                    await _tokenCacheService.StoreRefreshTokenSessionAsync(
                        old_refresh_jti,
                        userId.ToString(),
                        old_accessJti,
                        dbToken.ExpiredAt
                    );

                    await _tokenCacheService.AddUserSessionAsync(
                        userId.ToString(),
                        old_refresh_jti,
                        dbToken.ExpiredAt,
                        dbToken.LoginAt
                    );
                }
                catch (Exception)
                {
                    // Redis still offline, ignore warming up
                }
            }

            // 3. Perform Token rotation
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                if (!string.IsNullOrEmpty(old_accessJti))
                {
                    try
                    {
                        await _tokenCacheService.RemoveAccessTokenAsync(old_accessJti);
                    }
                    catch (Exception) { }
                }
                try
                {
                    await _tokenCacheService.RemoveRefreshTokenSessionAsync(old_refresh_jti);
                }
                catch (Exception) { }

                UserSessionDetail? oldSessionDetail = null;
                try
                {
                    oldSessionDetail = await _tokenCacheService.GetUserSessionDetailAsync(userId.ToString(), old_refresh_jti);
                }
                catch (Exception) { }

                var oldDbToken = await GetRefreshTokenByJtiAsync(old_refresh_jti);
                var loginAt = oldDbToken?.LoginAt ?? oldSessionDetail?.LoginAt ?? DateTime.UtcNow;

                try
                {
                    await _tokenCacheService.RemoveUserSessionAsync(userId.ToString(), old_refresh_jti);
                }
                catch (Exception) { }

                if (oldDbToken != null)
                {
                    oldDbToken.RevokeAndMarkUsed();
                    _refreshTokenRepository.Update(oldDbToken);
                }

                var new_access_jti = Guid.NewGuid().ToString();
                var new_refresh_jti = Guid.NewGuid().ToString();

                string? deviceId = null;
                if (jwtToken.TryGetClaim("device_id", out var devIdClaim))
                {
                    deviceId = devIdClaim?.Value;
                }
                deviceId ??= Guid.NewGuid().ToString();
                var accessTokenExpirationMinutes = GetAccessTokenExpirationMinutes();
                var refreshTokenExpirationDays = GetRefreshTokenExpirationDays();

                var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes);
                var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);

                var userEntity = await _userManager.FindByIdAsync(userId.ToString());
                if (userEntity == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<RefreshAccessTokenResponse>(
                        new Error("Auth.UserNotFound", "User not found."));
                }

                var accessTokenNew = await _tokenProvider.GenerateAccessTokenAsync(userEntity, new_access_jti, new_refresh_jti);
                var refreshTokenNew = _tokenProvider.GenerateRefreshToken(userEntity, new_refresh_jti, deviceId, refreshTokenExpiresAt);

                var reqDetail = GetRequestDetails();

                var newRefreshTokenEntity = RefreshToken.Create(
                    Guid.NewGuid(),
                    new_refresh_jti,
                    new_access_jti,
                    refreshTokenExpiresAt,
                    userEntity.Id,
                    reqDetail.DeviceName,
                    reqDetail.DeviceType,
                    reqDetail.Ip,
                    loginAt
                );

                userEntity.RefreshTokens ??= new List<RefreshToken>();
                userEntity.RefreshTokens.Add(newRefreshTokenEntity);

                var updateResult = await _userManager.UpdateAsync(userEntity);
                if (!updateResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<RefreshAccessTokenResponse>(
                        new Error("Auth.TokenSaveFailed", "Failed to save new session."));
                }

                try
                {
                    await _tokenCacheService.StoreAccessTokenAsync(new_access_jti, userId.ToString(), new_refresh_jti, accessTokenExpiresAt);
                    await _tokenCacheService.StoreRefreshTokenSessionAsync(new_refresh_jti, userId.ToString(), new_access_jti, refreshTokenExpiresAt);
                    await _tokenCacheService.AddUserSessionAsync(userId.ToString(), new_refresh_jti, refreshTokenExpiresAt, loginAt);
                }
                catch (Exception)
                {
                    // Redis offline, ignore
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                return Result.Success(new RefreshAccessTokenResponse(
                    refreshTokenNew,
                    refreshTokenExpiresAt,
                    accessTokenNew,
                    accessTokenExpiresAt
                ));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return Result.Failure<RefreshAccessTokenResponse>(
                    new Error("Auth.RefreshFailed", $"Token refresh failed: {ex.Message}"));
            }
        }

        private static string GetStableDeviceId(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent) || userAgent == "Unknown")
            {
                return Guid.NewGuid().ToString();
            }

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userAgent));
            var sb = new System.Text.StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public async Task<Result<LoginResponse>> GenerateTokenPairAndSaveAsync(AppUser user)
        {
            try
            {
                // 1. Cleanup stale sessions in Redis first
                try
                {
                    await _tokenCacheService.CleanupStaleAsync(user.Id.ToString());
                }
                catch (Exception) { }

                // 2. Check device limit (MAX 5)
                List<string> sessionJtis = new();
                bool redisSuccess = false;
                try
                {
                    sessionJtis = await _tokenCacheService.GetUserSessionsAsync(user.Id.ToString());
                    redisSuccess = true;
                }
                catch (Exception) { }

                if (!redisSuccess)
                {
                    // Fallback to DB when Redis is down
                    var dbTokens = await _refreshTokenRepository.FindAsync(
                        rt => rt.UserId == user.Id && !rt.IsRevoked && !rt.IsUsed && rt.ExpiredAt > DateTime.UtcNow,
                        CancellationToken.None);
                    var dbTokenList = dbTokens.ToList();

                    if (dbTokenList.Count >= 5)
                    {
                        var oldestDbToken = dbTokenList.OrderBy(t => t.LoginAt).FirstOrDefault();
                        if (oldestDbToken != null)
                        {
                            oldestDbToken.RevokeAndMarkUsed();
                            _refreshTokenRepository.Update(oldestDbToken);

                            // Attempt Redis cleanup in case it partially works or recovers
                            try
                            {
                                await _tokenCacheService.RemoveAccessTokenAsync(oldestDbToken.AccessJti);
                                await _tokenCacheService.RemoveRefreshTokenSessionAsync(oldestDbToken.Jti);
                                await _tokenCacheService.RemoveUserSessionAsync(user.Id.ToString(), oldestDbToken.Jti);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                else
                {
                    // Redis is working
                    if (sessionJtis.Count >= 5)
                    {
                        string? oldestJti = null;
                        DateTime oldestLoginAt = DateTime.MaxValue;

                        foreach (var jti in sessionJtis)
                        {
                            UserSessionDetail? sessionDetail = null;
                            try
                            {
                                sessionDetail = await _tokenCacheService.GetUserSessionDetailAsync(user.Id.ToString(), jti);
                            }
                            catch (Exception) { }

                            if (sessionDetail != null && sessionDetail.LoginAt < oldestLoginAt)
                            {
                                oldestLoginAt = sessionDetail.LoginAt;
                                oldestJti = jti;
                            }
                        }

                        if (oldestJti == null && sessionJtis.Any())
                        {
                            oldestJti = sessionJtis.First();
                        }

                        if (oldestJti != null)
                        {
                            try
                            {
                                var oldAccessJti = await _tokenCacheService.GetRefreshTokenSessionAsync(oldestJti);
                                if (!string.IsNullOrEmpty(oldAccessJti))
                                {
                                    await _tokenCacheService.RemoveAccessTokenAsync(oldAccessJti);
                                }
                                await _tokenCacheService.RemoveRefreshTokenSessionAsync(oldestJti);
                                await _tokenCacheService.RemoveUserSessionAsync(user.Id.ToString(), oldestJti);
                            }
                            catch (Exception) { }

                            var oldDbToken = await GetRefreshTokenByJtiAsync(oldestJti);
                            if (oldDbToken != null)
                            {
                                oldDbToken.RevokeAndMarkUsed();
                                _refreshTokenRepository.Update(oldDbToken);
                            }
                        }
                    }
                }

                // 3. Generate tokens
                var jti_access = Guid.NewGuid().ToString();
                var jti_session = Guid.NewGuid().ToString();
                var reqDetail = GetRequestDetails();
                var devId = GetStableDeviceId(reqDetail.DeviceName);

                var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes());
                var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays());

                var accessToken = await _tokenProvider.GenerateAccessTokenAsync(user, jti_access, jti_session);
                var refreshToken = _tokenProvider.GenerateRefreshToken(user, jti_session, devId, refreshTokenExpiresAt);

                user.RefreshTokens ??= new List<RefreshToken>();
                user.RefreshTokens.Add(RefreshToken.Create(
                    Guid.NewGuid(),
                    jti_session,
                    jti_access,
                    refreshTokenExpiresAt,
                    user.Id,
                    reqDetail.DeviceName,
                    reqDetail.DeviceType,
                    reqDetail.Ip,
                    DateTime.UtcNow
                ));

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return Result.Failure<LoginResponse>(
                        new Error("Auth.TokenSaveFailed", $"Failed to save session: {errors}"));
                }

                try
                {
                    await _tokenCacheService.StoreAccessTokenAsync(jti_access, user.Id.ToString(), jti_session, accessTokenExpiresAt);
                    await _tokenCacheService.StoreRefreshTokenSessionAsync(jti_session, user.Id.ToString(), jti_access, refreshTokenExpiresAt);
                    await _tokenCacheService.AddUserSessionAsync(user.Id.ToString(), jti_session, refreshTokenExpiresAt, DateTime.UtcNow);
                }
                catch (Exception)
                {
                    // Redis offline, ignore
                }

                return Result.Success(new LoginResponse(
                    accessToken,
                    accessTokenExpiresAt,
                    refreshToken,
                    refreshTokenExpiresAt
                ));
            }
            catch (Exception ex)
            {
                return Result.Failure<LoginResponse>(
                    new Error("Auth.TokenGenerationFailed", $"Failed to generate tokens: {ex.Message}"));
            }
        }
    }
}
