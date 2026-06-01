using MediatR;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ICacheDataService _redis;
        public static string Prefix = "MovieApp";

        public CachingBehavior(ICacheDataService cacheDataService)
        {
            _redis = cacheDataService;
        }

        public static string BuildKey(string baseKey,Dictionary<string,string>? version = null)
        {
            if (version == null || version.Count() <= 0)
                return $"{Prefix}:{baseKey}";

            var versionPart = string.Join(":",version.OrderBy(v => v.Key).Select(
                x => $"{x.Key}:v{x.Value}"));

            return $"{Prefix}:{baseKey}:{versionPart}";
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

            var finalKey = BuildKey(cacheable.CacheKey, versionDict);

            var getMethod = typeof(ICacheDataService)
                .GetMethod(nameof(ICacheDataService.GetAsync))
                .MakeGenericMethod(cacheable.ValueType);

            var cachedTask = (Task)getMethod.Invoke(_redis, new object[] { finalKey })!;
            await cachedTask;

            var cachedValue = cachedTask.GetType().GetProperty("Result")?.GetValue(cachedTask);

            if (cachedValue != null)
            {
                return CreateResult<TResponse>(cachedValue, cacheable.ValueType);
            }

            var response = await next();

            if (response is Result result && result.IsSuccess)
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

            return response;
        }
    }
}
