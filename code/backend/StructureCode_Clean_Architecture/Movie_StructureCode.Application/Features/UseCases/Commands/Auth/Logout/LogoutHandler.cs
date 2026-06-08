using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.Logout
{
    public sealed class LogoutHandler : ICommandHander<Logout.Command, bool>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ITokenCacheService _tokenCacheService;
        public LogoutHandler(
            UserManager<AppUser> userManager,
            IRefreshTokenService refreshTokenService,
            ITokenCacheService tokenCacheService)
        {
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
            _tokenCacheService = tokenCacheService;
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

                // 1. Remove Access Token and retrieve its parent SessionId
                var sessionId = await _tokenCacheService.RemoveAccessTokenAsync(command.jti);

                if (!string.IsNullOrEmpty(sessionId))
                {
                    // 2. Remove Refresh Token Session and User Session in Redis cache
                    await _tokenCacheService.RemoveRefreshTokenSessionAsync(sessionId);
                    await _tokenCacheService.RemoveUserSessionAsync(command.userId.ToString(), sessionId);

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
