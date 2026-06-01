using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.SeedData
{
    public static class RoomSeedData
    {
        // Theater 1 – CGV Vincom Center
        public static readonly Guid Room1Id = new("44444444-0000-0000-0000-000000000001");
        public static readonly Guid Room2Id = new("44444444-0000-0000-0000-000000000002");
        public static readonly Guid Room3Id = new("44444444-0000-0000-0000-000000000003");

        // Theater 2 – Lotte Cinema Landmark
        public static readonly Guid Room4Id = new("44444444-0000-0000-0000-000000000004");
        public static readonly Guid Room5Id = new("44444444-0000-0000-0000-000000000005");

        // Theater 3 – Galaxy Nguyen Du
        public static readonly Guid Room6Id = new("44444444-0000-0000-0000-000000000006");
        public static readonly Guid Room7Id = new("44444444-0000-0000-0000-000000000007");

        public static IEnumerable<Room> GetSeedData() =>
        [
            Room.Create(
                Room1Id,
                "Room 01",
                80,
                8,
                10,
                TheaterSeedData.Theater1Id,
                isActive: true
            ),
            Room.Create(
                Room2Id,
                "Room 02",
                60,
                6,
                10,
                TheaterSeedData.Theater1Id,
                isActive: true
            ),
            Room.Create(
                Room3Id,
                "Room 03 (IMAX)",
                120,
                10,
                12,
                TheaterSeedData.Theater1Id,
                isActive: true
            ),
            // -- Theater 2
            Room.Create(
                Room4Id,
                "Room 01",
                80,
                8,
                10,
                TheaterSeedData.Theater2Id,
                isActive: true
            ),
            Room.Create(
                Room5Id,
                "Room 02",
                60,
                6,
                10,
                TheaterSeedData.Theater2Id,
                isActive: true
            ),
            // -- Theater 3
            Room.Create(
                Room6Id,
                "Room 01",
                70,
                7,
                10,
                TheaterSeedData.Theater3Id,
                isActive: true
            ),
            Room.Create(
                Room7Id,
                "Room 02",
                50,
                5,
                10,
                TheaterSeedData.Theater3Id,
                isActive: true
            )
        ];
    }
}
