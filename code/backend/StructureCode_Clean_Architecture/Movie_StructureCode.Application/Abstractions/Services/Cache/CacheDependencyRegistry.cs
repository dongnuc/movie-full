using Movie_StructureCode.Application.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomLayout;

namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public static class CacheDependencyRegistry
    {
        private static readonly Dictionary<Type, List<CacheDependency>> _dependencies = new()
        {
            #region Room
            [typeof(GetRoomLayoutQuery)] = new()
            {
                new CacheDependency
                {
                    Entity = "Room",
                    IsGlobal = false,
                    IdSelector = request => ((GetRoomLayoutQuery)request).Id
                }
            },
            #endregion

        };

        public static List<CacheDependency> GetDependencies(object request)
        {
            var key = request.GetType();
            return _dependencies.TryGetValue(key, out var deps) ? deps : new List<CacheDependency>();
        }

    }
}
