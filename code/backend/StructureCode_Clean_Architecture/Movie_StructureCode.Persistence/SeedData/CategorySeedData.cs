using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.SeedData
{
    public static class CategorySeedData
    {
        public static readonly Guid ActionId = new("11111111-0000-0000-0000-000000000001");
        public static readonly Guid HorrorId = new("11111111-0000-0000-0000-000000000002");
        public static readonly Guid ComedyId = new("11111111-0000-0000-0000-000000000003");
        public static readonly Guid RomanceId = new("11111111-0000-0000-0000-000000000004");
        public static readonly Guid AnimationId = new("11111111-0000-0000-0000-000000000005");
        public static readonly Guid SciFiId = new("11111111-0000-0000-0000-000000000006");
        public static readonly Guid ThrillerID = new("11111111-0000-0000-0000-000000000007");

        public static IEnumerable<Category> GetSeedData() =>
        [
            Category.Create(
                ActionId,
                "Action",
                isActive: true
            ),
            Category.Create(
                HorrorId,
                "Horror",
                isActive: true),
            Category.Create(
                ComedyId,
                "Comedy",
                isActive: true
            ),
            Category.Create(
                RomanceId,
                "Romance",
                isActive: true
            ),
            Category.Create(
                AnimationId,
                "Animation",
                isActive: true
            ),
            Category.Create(
                SciFiId,
                "Sci-Fi",
                isActive: true
            ),
            Category.Create(
                ThrillerID,
                "Thriller",
                isActive: true
            )
        ];
    }
}
