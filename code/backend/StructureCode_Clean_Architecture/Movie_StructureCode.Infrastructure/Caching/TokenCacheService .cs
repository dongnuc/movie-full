using Microsoft.Extensions.Options;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Contract.Abstractions.Command;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Movie_StructureCode.Infrastructure.Caching
{
    public class TokenCacheService : ITokenCacheService
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _redis;
        private readonly string _prefix;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenCacheService(
            IConnectionMultiplexer connection,
            IOptions<RedisSettings> options,
            IHttpContextAccessor httpContextAccessor)
        {
            var settings = options.Value;

            _connection = connection;
            _redis = connection.GetDatabase(settings.DefaultDatabase);
            _prefix = settings.InstanceName ?? string.Empty;
            _httpContextAccessor = httpContextAccessor;
        }

        private string BuildKey(string key) => $"{_prefix}{key}";

        private static TimeSpan GetTtl(DateTime expiry)
        {
            var ttl = expiry - DateTime.UtcNow;
            return ttl <= TimeSpan.Zero ? TimeSpan.Zero : ttl;
        }

        private UserSessionDetail GetSessionDetail(DateTime? customLoginAt = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
            var ip = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            
            var deviceType = userAgent.Contains("Mobi", StringComparison.OrdinalIgnoreCase) ? "mobile" : "desktop";
            
            return new UserSessionDetail
            {
                DeviceName = userAgent,
                DeviceType = deviceType,
                Ip = ip,
                LoginAt = customLoginAt ?? DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };
        }


        private void EnsureConnected()
        {
            if (!_connection.IsConnected)
            {
                throw new InvalidOperationException("Redis connection is down.");
            }
        }

        // =========================
        // ACCESS TOKEN
        // =========================

        public async Task StoreAccessTokenAsync(string accessJti, string userId, string sessionId, DateTime expiry)
{
    EnsureConnected();

    var ttl = GetTtl(expiry);
    if (ttl == TimeSpan.Zero) return;

    var key = BuildKey($"auth:access:{accessJti}");
    HashEntry[] entries = new[]
    {
        new HashEntry("userId", userId),
        new HashEntry("sessionId", sessionId)
    };

    var batch = _redis.CreateBatch();
    var t1 = batch.HashSetAsync(key, entries);
    var t2 = batch.KeyExpireAsync(key, ttl);
    batch.Execute();
    await Task.WhenAll(t1, t2);
}

        public async Task<bool> IsAccessTokenValidAsync(string accessJti)
        {
            EnsureConnected();

            var key = BuildKey($"auth:access:{accessJti}");
            return await _redis.KeyExistsAsync(key);
        }

        public async Task<string?> RemoveAccessTokenAsync(string accessJti)
        {
            EnsureConnected();

            var key = BuildKey($"auth:access:{accessJti}");
            var sessionId = await _redis.HashGetAsync(key, "sessionId");
            if (sessionId.IsNullOrEmpty) return null;

            await _redis.KeyDeleteAsync(key);
            return sessionId.ToString();
        }

        // =========================
        // REFRESH TOKEN (SESSION)
        // =========================

        public async Task StoreRefreshTokenSessionAsync(string refreshJti, string userId, string accessJti, DateTime expiry)
{
    EnsureConnected();

    var ttl = GetTtl(expiry);
    if (ttl == TimeSpan.Zero) return;

    var key = BuildKey($"auth:session:{refreshJti}");
    HashEntry[] entries = new[]
    {
        new HashEntry("userId", userId),
        new HashEntry("accessJti", accessJti)
    };

    var batch = _redis.CreateBatch();
    var t1 = batch.HashSetAsync(key, entries);
    var t2 = batch.KeyExpireAsync(key, ttl);
    batch.Execute();
    await Task.WhenAll(t1, t2);
}

        public async Task<string?> GetRefreshTokenSessionAsync(string refreshJti)
        {
            EnsureConnected();

            var key = BuildKey($"auth:session:{refreshJti}");
            var accessJti = await _redis.HashGetAsync(key, "accessJti");
            return accessJti.IsNullOrEmpty ? null : accessJti.ToString();
        }

        public async Task RemoveRefreshTokenSessionAsync(string refreshJti)
        {
            EnsureConnected();

            var key = BuildKey($"auth:session:{refreshJti}");
            await _redis.KeyDeleteAsync(key);
        }

        // =========================
        // USER SESSIONS INDEX
        // =========================

        public async Task AddUserSessionAsync(string userId, string refreshJti, DateTime expiry, DateTime? loginAt = null)
{
    EnsureConnected();

    var key = BuildKey($"auth:user:{userId}:sessions");
    var detail = GetSessionDetail(loginAt);
    var value = System.Text.Json.JsonSerializer.Serialize(detail);

    var ttl = GetTtl(expiry);

    var batch = _redis.CreateBatch();
    var hashSetTask = batch.HashSetAsync(key, refreshJti, value);
    var ttlReadTask = batch.KeyTimeToLiveAsync(key);
    batch.Execute();

    await hashSetTask;
    var currentTtl = await ttlReadTask;

    if (ttl > TimeSpan.Zero && (currentTtl == null || ttl > currentTtl))
    {
        await _redis.KeyExpireAsync(key, ttl);
    }
}
        public async Task<UserSessionDetail?> GetUserSessionDetailAsync(string userId, string refreshJti)
        {
            EnsureConnected();

            var key = BuildKey($"auth:user:{userId}:sessions");
            var value = await _redis.HashGetAsync(key, refreshJti);
            if (value.IsNullOrEmpty) return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<UserSessionDetail>(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public async Task RemoveUserSessionAsync(string userId, string refreshJti)
        {
            EnsureConnected();

            var key = BuildKey($"auth:user:{userId}:sessions");
            await _redis.HashDeleteAsync(key, refreshJti);
        }

        public async Task<List<string>> GetUserSessionsAsync(string userId)
        {
            EnsureConnected();

            var key = BuildKey($"auth:user:{userId}:sessions");
            var fields = await _redis.HashKeysAsync(key);
            return fields.Select(f => f.ToString()).ToList();
        }

        public async Task RemoveAllUserSessionsAsync(string userId)
        {
            EnsureConnected();

            var key = BuildKey($"auth:user:{userId}:sessions");
            await _redis.KeyDeleteAsync(key);
        }

        public async Task CleanupStaleAsync(string userId)
        {
            EnsureConnected();

            var key = BuildKey($"auth:user:{userId}:sessions");
            var entries = await _redis.HashGetAllAsync(key);
            foreach (var entry in entries)
            {
                var sessionKey = BuildKey($"auth:session:{entry.Name}");
                var exists = await _redis.KeyExistsAsync(sessionKey);
                if (!exists)
                {
                    await _redis.HashDeleteAsync(key, entry.Name);
                }
            }
        }
    }
}