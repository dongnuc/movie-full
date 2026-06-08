using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public interface ITokenCacheService
    {
        // ===== ACCESS TOKEN =====
        Task StoreAccessTokenAsync(string accessJti, string userId, string sessionId, DateTime expiry);
        Task<bool> IsAccessTokenValidAsync(string accessJti);
        Task<string?> RemoveAccessTokenAsync(string accessJti);

        // ===== REFRESH TOKEN (SESSION) =====
        Task StoreRefreshTokenSessionAsync(string refreshJti, string userId, string accessJti, DateTime expiry);
        Task<string?> GetRefreshTokenSessionAsync(string refreshJti);
        Task RemoveRefreshTokenSessionAsync(string refreshJti);

        // ===== USER SESSIONS INDEX =====
        Task AddUserSessionAsync(string userId, string refreshJti, DateTime expiry, DateTime? loginAt = null);
        Task<UserSessionDetail?> GetUserSessionDetailAsync(string userId, string refreshJti);
        Task RemoveUserSessionAsync(string userId, string refreshJti);
        Task<List<string>> GetUserSessionsAsync(string userId);
        Task RemoveAllUserSessionsAsync(string userId);
        Task CleanupStaleAsync(string userId);
    }

    public class UserSessionDetail
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public DateTime LoginAt { get; set; }
        public DateTime LastUsedAt { get; set; }
    }
}
