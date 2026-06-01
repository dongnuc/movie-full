using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Application.Abstractions.Services.Cache;

namespace Movie_StructureCode.Application.Features.UseCases.Commands.Auth.RefreshAccessToken
{
    public sealed class RefreshAccessTokenHandler : ICommandHander<RefreshAccessToken.Command, RefreshAccessTokenResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenProvider _tokenProvider;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly IRefreshTokenService _refreshTokenService;

        public RefreshAccessTokenHandler(
            UserManager<AppUser> userManager,
            ITokenProvider tokenProvider,
            ITokenCacheService tokenCacheService,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _tokenProvider = tokenProvider;
            _tokenCacheService = tokenCacheService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<RefreshAccessTokenResponse>> Handle(
            RefreshAccessToken.Command command,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate refresh token
                var refreshToken = await _refreshTokenService.GetRefreshTokenByTokenAsync(command.RefreshToken);

                if (refreshToken == null ||
                    refreshToken.IsRevoked ||
                    refreshToken.IsUsed || 
                    refreshToken.ExpriseDate <= DateTime.UtcNow)
                {
                    return Result.Failure<RefreshAccessTokenResponse>(
                        new Error("Auth.InvalidRefreshToken", "Invalid or expired refresh token."));
                }

                // Get user
                var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
                if (user == null)
                    return Result.Failure<RefreshAccessTokenResponse>(
                        new Error("Auth.UserNotFound", "User not found."));

                // mark used refreshToken
                await _refreshTokenService.RevokeRefreshTokenAsync(command.RefreshToken);

                // jti older
                var oldJti = refreshToken.JwtId;
                //redis remove
                await _tokenCacheService.RemoveAccessTokenAsync(oldJti);
                await _tokenCacheService.RemoveUserSessionAsync(refreshToken.UserId.ToString(), oldJti);

                // generate new token 
                var newJti = Guid.NewGuid().ToString();

                // Generate new access token
                var accessToken = await _tokenProvider.GenerateAccessTokenAsync(user, newJti);
                var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(2);
                var refreshTokenNew = _refreshTokenService.GenerateRefreshToken();
                var refreshTokenExpires = DateTime.UtcNow.AddDays(7);

                var refreshTokenEntity = RefreshToken.Create(
                    Guid.NewGuid(),
                    refreshTokenNew,
                    newJti,
                    refreshTokenExpires,
                    user.Id
                );
                user.RefreshTokens ??= new List<RefreshToken>();
                user.RefreshTokens.Add(refreshTokenEntity);

                await _userManager.UpdateAsync(user);

                var response = new RefreshAccessTokenResponse(
                    refreshTokenNew,
                    refreshTokenExpires,
                    accessToken,
                    accessTokenExpiresAt
                );

                // add to redis
                await _tokenCacheService.StoreAccessTokenAsync(newJti, user.Id.ToString(),accessTokenExpiresAt);
                await _tokenCacheService.AddUserSessionAsync(user.Id.ToString(), newJti,accessTokenExpiresAt);

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<RefreshAccessTokenResponse>(
                    new Error("Auth.RefreshFailed", $"Token refresh failed: {ex.Message}"));
            }
        }
    }
}
