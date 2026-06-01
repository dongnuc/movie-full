using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using System.Security.Cryptography;

namespace Movie_StructureCode.Infrastructure.Services.Auth
{
    /// <summary>
    /// Refresh token service implementation - x? l� refresh token generation, validation, revocation
    /// Ch?u tr�ch nhi?m: T?o token (RNG), validate from DB, revoke token
    /// </summary>
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        public RefreshTokenService(IRepository<RefreshToken> refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Generate refresh token using cryptographically secure RNG
        /// Returns Base64 encoded 64-byte random string
        /// </summary>
        public string GenerateRefreshToken()
        {
            try
            {
                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);

                return Convert.ToBase64String(randomBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating refresh token", ex);
            }
        }

        /// <summary>
        /// Validate refresh token from database
        /// Check: token exists, not revoked, not expired
        /// </summary>
        public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string token)
        {
            try
            {
                var refreshTokens = await _refreshTokenRepository.FindAsync(
                    rt => rt.UserId == userId && rt.Token == token,
                    CancellationToken.None);

                if (!refreshTokens.Any())
                    return false;

                var refreshTokenEntity = refreshTokens.FirstOrDefault();
                
                // Check if revoked
                if (refreshTokenEntity?.IsRevoked == true)
                    return false;

                // Check if expired
                if (refreshTokenEntity?.ExpriseDate < DateTime.UtcNow)
                    return false;

                // Update LastUsedDate
                refreshTokenEntity!.UpdateLastUsed(DateTime.UtcNow);
                _refreshTokenRepository.Update(refreshTokenEntity);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Revoke single refresh token (mark as revoked in database)
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            try
            {
                var refreshTokens = await _refreshTokenRepository.FindAsync(
                    rt => rt.Token == token,
                    CancellationToken.None);

                if (!refreshTokens.Any())
                    return false;

                var refreshToken = refreshTokens.FirstOrDefault();
                if (refreshToken == null)
                    return false;

                refreshToken.RevokeAndMarkUsed();
                _refreshTokenRepository.Update(refreshToken);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Revoke all refresh tokens of user (e.g., on logout)
        /// </summary>
        public async Task<bool> RevokeAllRefreshTokensAsync(Guid userId)
        {
            try
            {
                var refreshTokens = await _refreshTokenRepository.FindAsync(
                    rt => rt.UserId == userId && !rt.IsRevoked,
                    CancellationToken.None);

                if (!refreshTokens.Any())
                    return true; // No tokens to revoke

                foreach (var refreshToken in refreshTokens)
                {
                    refreshToken.Revoke();
                    _refreshTokenRepository.Update(refreshToken);
                    await _unitOfWork.SaveChangesAsync();

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get refresh token information by token string
        /// Returns RefreshToken entity with all properties (Id, Token, JwtId, CreatedDate, LastUsedDate, ExpriseDate, IsRevoked, UserId)
        /// Returns null if token not found
        /// </summary>
        public async Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token)
        {
            try
            {
                var refreshTokens = await _refreshTokenRepository.FindAsync(
                    rt => rt.Token == token,
                    CancellationToken.None);

                if (!refreshTokens.Any())
                    return null;

                var refreshToken = refreshTokens.FirstOrDefault();

                // Return RefreshToken entity information as anonymous object or RefreshToken object
                return refreshToken;
            }
            catch
            {
                return null;
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenByJtiAsync(string jti)
        {
            try
            {
                var refreshTokens = await _refreshTokenRepository.FindAsync(rf => rf.JwtId == jti, CancellationToken.None);

                if (!refreshTokens.Any())
                    return null;

                var result = refreshTokens.FirstOrDefault(); return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
