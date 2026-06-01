using Microsoft.Extensions.Options;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Contract.Abstractions.Command;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace Movie_StructureCode.Infrastructure.Caching
{
    public class TokenCacheService : ITokenCacheService
    {
        private readonly IDatabase _redis;
        private readonly string _prefix;

        public TokenCacheService(
            IConnectionMultiplexer connection,
            IOptions<RedisSettings> options)
        {
            var settings = options.Value;

            _redis = connection.GetDatabase(settings.DefaultDatabase);
            _prefix = settings.InstanceName ?? string.Empty;
        }

        // =========================
        // PRIVATE HELPERS
        // =========================

        private string BuildKey(string key) => $"{_prefix}{key}";

        private static TimeSpan GetTtl(DateTime expiry)
        {
            var ttl = expiry - DateTime.UtcNow;
            return ttl <= TimeSpan.Zero ? TimeSpan.Zero : ttl;
        }

        //  Hash refresh token (security)
        private static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        // =========================
        // ACCESS TOKEN
        // =========================

        public async Task StoreAccessTokenAsync(string jti, string userId, DateTime expiry)
        {
            var ttl = GetTtl(expiry);
            if (ttl == TimeSpan.Zero) return;

            var key = BuildKey($"access_token:{jti}");

            await _redis.StringSetAsync(key, userId, ttl);
        }

        public async Task<bool> IsAccessTokenValidAsync(string jti)
        {
            var key = BuildKey($"access_token:{jti}");
            return await _redis.KeyExistsAsync(key);
        }

        public async Task RemoveAccessTokenAsync(string jti)
        {
            var key = BuildKey($"access_token:{jti}");
            await _redis.KeyDeleteAsync(key);
        }

        // =========================
        // REFRESH TOKEN
        // =========================

        public async Task StoreRefreshTokenAsync(string refreshToken, string userId, DateTime expiry)
        {
            var ttl = GetTtl(expiry);
            if (ttl == TimeSpan.Zero) return;

            var hashedToken = HashToken(refreshToken);
            var key = BuildKey($"refresh_token:{hashedToken}");

            await _redis.StringSetAsync(key, userId, ttl);
        }

        public async Task<bool> IsRefreshTokenExistsAsync(string refreshToken)
        {
            var hashedToken = HashToken(refreshToken);
            var key = BuildKey($"refresh_token:{hashedToken}");

            return await _redis.KeyExistsAsync(key);
        }

        public async Task RemoveRefreshTokenAsync(string refreshToken)
        {
            var hashedToken = HashToken(refreshToken);
            var key = BuildKey($"refresh_token:{hashedToken}");

            await _redis.KeyDeleteAsync(key);
        }

        // =========================
        // SESSION (MULTI DEVICE)
        // =========================

        public async Task AddUserSessionAsync(string userId, string jti, DateTime expiry)
        {
            var ttl = GetTtl(expiry);
            if (ttl == TimeSpan.Zero) return;

            var key = BuildKey($"user_sessions:{userId}");
            await _redis.SetAddAsync(key, jti);
            await _redis.KeyExpireAsync(key, ttl);
        }

        public async Task RemoveUserSessionAsync(string userId, string jti)
        {
            var key = BuildKey($"user_sessions:{userId}");
            await _redis.SetRemoveAsync(key, jti);
        }

        public async Task<List<string>> GetUserSessionsAsync(string userId)
        {
            var key = BuildKey($"user_sessions:{userId}");
            var values = await _redis.SetMembersAsync(key);

            return values.Select(v => v.ToString()).ToList();
        }

        public async Task RemoveAllUserSessionsAsync(string userId)
        {
            var key = BuildKey($"user_sessions:{userId}");
            await _redis.KeyDeleteAsync(key);
        }
    }
}