namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public interface ITokenCacheService
    {
        // ===== ACCESS TOKEN =====
        Task StoreAccessTokenAsync(string jti, string userId, DateTime expiry);
        Task<bool> IsAccessTokenValidAsync(string jti);
        Task RemoveAccessTokenAsync(string jti);

        // ===== REFRESH TOKEN =====
        Task StoreRefreshTokenAsync(string refreshToken, string userId, DateTime expiry);
        Task<bool> IsRefreshTokenExistsAsync(string refreshToken);
        Task RemoveRefreshTokenAsync(string refreshToken);

        // ===== SESSION =====
        Task AddUserSessionAsync(string userId, string jti, DateTime expiry);
        Task RemoveUserSessionAsync(string userId, string jti);
        Task<List<string>> GetUserSessionsAsync(string userId);
        Task RemoveAllUserSessionsAsync(string userId);
    }
}
