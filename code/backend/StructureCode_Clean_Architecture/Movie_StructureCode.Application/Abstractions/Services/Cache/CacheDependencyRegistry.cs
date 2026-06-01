using Movie_StructureCode.Application.Common;

namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public static class CacheDependencyRegistry
    {
        private static readonly Dictionary<Type, List<CacheDependency>> _dependencies = new()
        {
        };

        public static List<CacheDependency> GetDependencies(object request)
        {
            var key = request.GetType();
            return _dependencies.TryGetValue(key, out var deps) ? deps : new List<CacheDependency>();
        }

    }
}
