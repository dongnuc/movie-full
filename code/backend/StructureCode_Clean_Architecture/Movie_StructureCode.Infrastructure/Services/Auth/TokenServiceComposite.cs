using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Features.UseCases.Commands.Auth.RefreshAccessToken;
using Movie_StructureCode.Application.Features.UseCases.Queries.Auth.Login;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Infrastructure.Services.Auth
{
    public sealed class TokenServiceComposite : ITokenService
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly IRefreshTokenService _refreshTokenService;

        public TokenServiceComposite(
            ITokenProvider tokenProvider,
            IRefreshTokenService refreshTokenService)
        {
            _tokenProvider = tokenProvider;
            _refreshTokenService = refreshTokenService;
        }

        // ITokenProvider delegation
        public Task<string> GenerateAccessTokenAsync(AppUser user, string jti, string sessionId)
            => _tokenProvider.GenerateAccessTokenAsync(user, jti, sessionId);

        public ClaimsPrincipal? ValidateAccessToken(string token)
            => _tokenProvider.ValidateAccessToken(token);

        public ClaimsPrincipal? DecodeToken(string token)
            => _tokenProvider.DecodeToken(token);

        public ClaimsPrincipal? DecodeToken(string token, out SecurityToken? validatedToken)
            => _tokenProvider.DecodeToken(token, out validatedToken);

        public string GenerateRefreshToken(AppUser user, string refreshJti, string deviceId, DateTime expiresAt)
            => _tokenProvider.GenerateRefreshToken(user, refreshJti, deviceId, expiresAt);

        public ClaimsPrincipal? DecodeRefreshToken(string token, out SecurityToken? validatedToken)
            => _tokenProvider.DecodeRefreshToken(token, out validatedToken);

        // IRefreshTokenService delegation
        public double GetAccessTokenExpirationMinutes()
            => _refreshTokenService.GetAccessTokenExpirationMinutes();

        public double GetRefreshTokenExpirationDays()
            => _refreshTokenService.GetRefreshTokenExpirationDays();

        public Task<bool> RevokeRefreshTokenAsync(RefreshToken token)
            => _refreshTokenService.RevokeRefreshTokenAsync(token);

        public Task<bool> RevokeAllRefreshTokensAsync(Guid userId)
            => _refreshTokenService.RevokeAllRefreshTokensAsync(userId);

        public Task<RefreshToken?> GetRefreshTokenByJtiAsync(string jti)
            => _refreshTokenService.GetRefreshTokenByJtiAsync(jti);

        public Task<Result<RefreshAccessTokenResponse>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => _refreshTokenService.RefreshAccessTokenAsync(refreshToken, cancellationToken);

        public Task<Result<LoginResponse>> GenerateTokenPairAndSaveAsync(AppUser user)
            => _refreshTokenService.GenerateTokenPairAndSaveAsync(user);
    }
}
