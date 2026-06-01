using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Abstractions.Services.Auth
{
    /// <summary>
    /// Refresh token service interface - x? lý refresh token generation, validation, revocation
    /// Ch?u trách nhi?m: T?o token, validate, revoke token
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Generate refresh token (random secure string)
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate refresh token t? database
        /// </summary>
        Task<bool> ValidateRefreshTokenAsync(Guid userId, string token);

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string token);

        /// <summary>
        /// Revoke all refresh tokens c?a user
        /// </summary>
        Task<bool> RevokeAllRefreshTokensAsync(Guid userId);

        /// <summary>
        /// Get refresh token information by token string
        /// Returns full RefreshToken entity information (Id, JwtId, CreatedDate, LastUsedDate, ExpriseDate, IsRevoked, UserId)
        /// </summary>
        Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token);
        Task<RefreshToken?> GetRefreshTokenByJtiAsync(string jti);

    }
}
