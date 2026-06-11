using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.Logout
{
    public sealed class LogoutHandler : ICommandHander<Logout.Command, bool>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;

        public LogoutHandler(
            UserManager<AppUser> userManager,
            IRefreshTokenService refreshTokenService,
            ITokenCacheService tokenCacheService,
            IRepository<RefreshToken> refreshTokenRepository)
        {
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
            _tokenCacheService = tokenCacheService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Result<bool>> Handle(
            Logout.Command command,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(command.userId.ToString());
                if (user == null)
                {
                    return Result.Failure<bool>(
                        new Error("Auth.UserNotFound", "User not found."));
                }

                string? sessionId = null;
                try
                {
                    // 1. Remove Access Token and retrieve its parent SessionId
                    sessionId = await _tokenCacheService.RemoveAccessTokenAsync(command.jti);
                }
                catch (Exception)
                {
                    // Redis connection down, skip and fallback to DB
                }

                if (string.IsNullOrEmpty(sessionId))
                {
                    // Fallback to DB: find RefreshToken using the Access Token JTI
                    var tokens = await _refreshTokenRepository.FindAsync(
                        rt => rt.AccessJti == command.jti && !rt.IsRevoked,
                        cancellationToken);
                    var dbToken = tokens.FirstOrDefault();
                    if (dbToken != null)
                    {
                        sessionId = dbToken.Jti;
                        await _refreshTokenService.RevokeRefreshTokenAsync(dbToken);
                    }
                }
                else
                {
                    try
                    {
                        // 2. Remove Refresh Token Session and User Session in Redis cache
                        await _tokenCacheService.RemoveRefreshTokenSessionAsync(sessionId);
                        await _tokenCacheService.RemoveUserSessionAsync(command.userId.ToString(), sessionId);
                    }
                    catch (Exception) { }

                    // 3. Revoke and update refresh token in SQL database
                    var refreshTokenOld = await _refreshTokenService.GetRefreshTokenByJtiAsync(sessionId);
                    if (refreshTokenOld != null)
                    {
                        await _refreshTokenService.RevokeRefreshTokenAsync(refreshTokenOld);
                    }
                }

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>(
                    new Error("Auth.LogoutFailed", $"Logout failed: {ex.Message}"));
            }
        }
    }
}
