using MediatR;
using Microsoft.Extensions.Logging;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ICacheDataService _redis;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
        public static string Prefix = "MovieApp";

        public CachingBehavior(ICacheDataService cacheDataService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _redis = cacheDataService;
            _logger = logger;
        }

        public static string BuildKey(string baseKey, Dictionary<string, string>? version = null)
        {
            if (version == null || version.Count == 0)
                return $"{Prefix}:{baseKey}";

            var versionParts = new List<string>();
            foreach (var kv in version.OrderBy(v => v.Key))
            {
                var keyParts = kv.Key.Split(':');
                if (keyParts.Length == 2)
                {
                    var entity = keyParts[0];
                    var id = keyParts[1];

                    // If baseKey starts with the entity name (e.g., "Room:"), we append the ID without duplicating the entity name
                    if (baseKey.StartsWith(entity + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        versionParts.Add($"{id}:v{kv.Value}");
                    }
                    else
                    {
                        versionParts.Add($"{kv.Key}:v{kv.Value}");
                    }
                }
                else
                {
                    versionParts.Add($"{kv.Key}:v{kv.Value}");
                }
            }

            return $"{Prefix}:{baseKey}:{string.Join(":", versionParts)}";
        }


        private static TResponse CreateResult<TResponse>(object value, Type valueType)
        {
            var resultType = typeof(Result<>).MakeGenericType(valueType);


            var method = typeof(Result)
                .GetMethods()
                .First(x => x.Name == "Success" && x.IsGenericMethod)
                .MakeGenericMethod(valueType);

            var result = method.Invoke(null, new[] { value })!;

            return (TResponse)result;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not ICacheableQuery cacheable || cacheable.BypassCache)
                return await next();

            string? finalKey = null;

            try
            {
                var dependencies = CacheDependencyRegistry.GetDependencies(request);
                var versionDict = new Dictionary<string, string>();

                foreach (var dep in dependencies)
                {
                    if (dep.IsGlobal)
                    {
                        var v =  await _redis.GetTagVersionAsync(dep.Entity);
                        versionDict[dep.Entity] = v;
                    }
                    if(dep.IdSelector != null)
                    {
                        var id = dep.IdSelector(request);
                        if (id.HasValue)
                        {
                            var v = await _redis.GetEntityVersionAsync(dep.Entity, id.Value);
                            versionDict[$"{dep.Entity}:{id.Value}"] = v;
                        }
                    }
                }

                finalKey = BuildKey(cacheable.CacheKey, versionDict);

                var getMethod = typeof(ICacheDataService)
                    .GetMethod(nameof(ICacheDataService.GetAsync))
                    .MakeGenericMethod(cacheable.ValueType);

                var cachedTask = (Task)getMethod.Invoke(_redis, new object[] { finalKey })!;
                await cachedTask;

                var cachedValue = cachedTask.GetType().GetProperty("Result")?.GetValue(cachedTask);

                if (cachedValue != null)
                {
                    _logger.LogInformation("--> [CACHE HIT] Retrieved data from Redis for key: {CacheKey}", finalKey);
                    return CreateResult<TResponse>(cachedValue, cacheable.ValueType);
                }

                _logger.LogInformation("--> [CACHE MISS] Key not found in Redis: {CacheKey}. Fetching from database.", finalKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "--> [CACHE ERROR] Redis connection failed or timed out. Falling back to DB for query {QueryType}", typeof(TRequest).Name);
            }

            var response = await next();

            if (finalKey != null && response is Result result && result.IsSuccess)
            {
                try
                {
                    var value = response.GetType().GetProperty("Value")?.GetValue(response);

                    if (value != null)
                    {
                        var setMethod = typeof(ICacheDataService)
                            .GetMethod(nameof(ICacheDataService.SetAsync))!
                            .MakeGenericMethod(cacheable.ValueType);

                        var setTask = (Task)setMethod.Invoke(_redis,
                            new object[] { finalKey, value, cacheable.Expiration })!;

                        await setTask;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to write cache to Redis for key {CacheKey}", finalKey);
                }
            }

            return response;
        }
    }
}
