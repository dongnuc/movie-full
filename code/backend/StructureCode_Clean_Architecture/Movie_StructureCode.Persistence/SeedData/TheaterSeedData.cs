using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.SeedData
{
    public static class TheaterSeedData
    {
        public static readonly Guid Theater1Id = new("33333333-0000-0000-0000-000000000001");
        public static readonly Guid Theater2Id = new("33333333-0000-0000-0000-000000000002");
        public static readonly Guid Theater3Id = new("33333333-0000-0000-0000-000000000003");

        public static IEnumerable<Theater> GetSeedData() =>
        [
            Theater.Create(
                Theater1Id,
                "CGV Vincom Center",
                "72 Le Thanh Ton, Ben Nghe, District 1, Ho Chi Minh City",
                isActive: true
            ),
            Theater.Create(
                Theater2Id,
                "Lotte Cinema Landmark",
                "772B Dien Bien Phu, Ward 22, Binh Thanh, Ho Chi Minh City",
                isActive: true
            ),
            Theater.Create(
                Theater3Id,
                "Galaxy Nguyen Du",
                "116 Nguyen Du, Ben Thanh, District 1, Ho Chi Minh City",
                isActive: true
            )
        ];
    }
}
