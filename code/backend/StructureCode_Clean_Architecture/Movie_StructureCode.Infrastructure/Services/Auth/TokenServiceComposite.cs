using System.Security.Claims;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Infrastructure.Services.Auth
{
    /// <summary>
    /// Token service composite - combines ITokenProvider and IRefreshTokenService
    /// Dùng ?? DI injection, delegates t?t c? calls ??n các specialized services
    /// </summary>
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

        // ITokenProvider methods
        //public async Task<string> GenerateAccessTokenAsync(AppUser user)
        //    => await _tokenProvider.GenerateAccessTokenAsync(user);

        public ClaimsPrincipal? ValidateAccessToken(string token)
            => _tokenProvider.ValidateAccessToken(token);

        public ClaimsPrincipal? DecodeToken(string token)
            => _tokenProvider.DecodeToken(token);

        // IRefreshTokenService methods
        public string GenerateRefreshToken()
            => _refreshTokenService.GenerateRefreshToken();

        public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string token)
            => await _refreshTokenService.ValidateRefreshTokenAsync(userId, token);

        public async Task<bool> RevokeRefreshTokenAsync( string token)
            => await _refreshTokenService.RevokeRefreshTokenAsync( token);

        public async Task<bool> RevokeAllRefreshTokensAsync(Guid userId)
            => await _refreshTokenService.RevokeAllRefreshTokensAsync(userId);

        public async Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token)
            => await _refreshTokenService.GetRefreshTokenByTokenAsync(token);

        public Task<string> GenerateAccessTokenAsync(AppUser user, string jti)
        => _tokenProvider.GenerateAccessTokenAsync(user, jti);

        public Task<RefreshToken?> GetRefreshTokenByJtiAsync(string jti)
        {
            throw new NotImplementedException();
        }
    }
}
