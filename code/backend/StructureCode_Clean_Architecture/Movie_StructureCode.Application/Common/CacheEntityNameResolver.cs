namespace Movie_StructureCode.Application.Common
{
    public static class CacheEntityNameResolver
    {
        private static readonly Dictionary<Type, string> Map = new()
        {
            [typeof(Domain.Entities.SeatType)] = "seattype",
            [typeof(Domain.Entities.Movie)] = "movie",
            [typeof(Domain.Entities.Showing)] = "showing",
            [typeof(Domain.Entities.Seat)] = "seat",
            [typeof(Domain.Entities.ShowingSeat)] = "showingseat"
        };

        public static string Resolve(Type type)
        {
            return Map.TryGetValue(type, out var name)
                ? name
                : type.Name.ToLower(); // fallback
        }
    }
}
