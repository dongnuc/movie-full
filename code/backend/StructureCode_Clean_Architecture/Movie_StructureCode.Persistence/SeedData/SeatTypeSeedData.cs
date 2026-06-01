using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.SeedData
{
    public static class SeatTypeSeedData
    {
        public static readonly Guid StandardId = new("55555555-0000-0000-0000-000000000001");
        public static readonly Guid VipId = new("55555555-0000-0000-0000-000000000002");
        public static readonly Guid CoupleId = new("55555555-0000-0000-0000-000000000003");

        public static IEnumerable<SeatType> GetSeedData() =>
        [
            SeatType.Create(
                StandardId,
                "Standard",
                surcharge: 0m,
                isActive: true),
            SeatType.Create(
                VipId,
                "VIP",
                surcharge: 30_000m,
                isActive: true),
            SeatType.Create(
                CoupleId,
                "Couple",
                surcharge: 50_000m,
                isActive: true
               )
        ];
    }
}
