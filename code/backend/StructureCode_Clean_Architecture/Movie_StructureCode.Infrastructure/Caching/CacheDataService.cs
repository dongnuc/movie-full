using Movie_StructureCode.Application.Abstractions.Services.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace Movie_StructureCode.Infrastructure.Caching
{
    public class CacheDataService : ICacheDataService
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _redis;

        public CacheDataService(IConnectionMultiplexer redis)
        {
            _connection = redis;
            _redis = redis.GetDatabase();   
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (!_connection.IsConnected)
                return default;

            var value = await _redis.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;
            
            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task<string> GetEntityVersionAsync(string entity, Guid id)
        {
            if (!_connection.IsConnected)
                return "0";

            var value = await _redis.StringGetAsync($"tag:{entity}:{id}");
            return value.IsNullOrEmpty ? "0" : value!;
        }

        public async Task<string> GetTagVersionAsync(string tag)
        {
            if (!_connection.IsConnected)
                return "0";

            var value = await _redis.StringGetAsync($"tag:{tag}");
            return value.IsNullOrEmpty ? "0" : value!;
        }

        public Task<long> InCrementEntityVersionAsync(string entity, Guid id)
        {
            if (!_connection.IsConnected)
                return Task.FromResult(0L);

            return _redis.StringIncrementAsync($"tag:{entity}:{id}");
        }

        public async Task<long> InCrementTagVersionAsync(string tag)
        {
            if (!_connection.IsConnected)
                return 0;

            //return await _redis.StringIncrementAsync($"tag:{tag}");
            var before = await _redis.StringGetAsync($"tag:{tag}");
            Console.WriteLine($"Before: {before}");

            var after = await _redis.StringIncrementAsync($"tag:{tag}");
            Console.WriteLine($"After: {after}");
            
            return after;
        }

        public async Task RemoveAsync(string key)
        {
            if (!_connection.IsConnected)
                return;

            await _redis.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration)
        {
            if (!_connection.IsConnected)
                return;

            var json = JsonSerializer.Serialize(value);

            if (expiration.HasValue)
            {
                await _redis.StringSetAsync(key, json, expiry: expiration.Value);
            }
            else
            {
                await _redis.StringSetAsync(key, json);
            }
        }
    }
}
