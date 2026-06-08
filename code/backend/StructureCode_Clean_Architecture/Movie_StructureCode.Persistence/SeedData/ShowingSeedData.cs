using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.SeedData
{
    public static class ShowingSeedData
    {
        public static readonly Guid Showing1Id = new("66666666-0000-0000-0000-000000000001");
        public static readonly Guid Showing2Id = new("66666666-0000-0000-0000-000000000002");
        public static readonly Guid Showing3Id = new("66666666-0000-0000-0000-000000000003");
        public static readonly Guid Showing4Id = new("66666666-0000-0000-0000-000000000004");
        public static readonly Guid Showing5Id = new("66666666-0000-0000-0000-000000000005");
        public static readonly Guid Showing6Id = new("66666666-0000-0000-0000-000000000006");
        public static readonly Guid Showing7Id = new("66666666-0000-0000-0000-000000000007");
        public static readonly Guid Showing8Id = new("66666666-0000-0000-0000-000000000008");
        public static readonly Guid Showing9Id = new("66666666-0000-0000-0000-000000000009");
        public static readonly Guid Showing10Id = new("66666666-0000-0000-0000-000000000010");

        public static IEnumerable<Showing> GetSeedData() =>
        [
            // -- Room 1
            Showing.Create(
                Showing1Id,
                new DateTime(2025, 6, 1,  9, 0, 0, DateTimeKind.Utc),
                85_000m,
                8,
                MovieSeedData.Movie1Id,
                RoomSeedData.Room1Id,
                TheaterSeedData.Theater1Id,
                timeDuration: 107,
                isActive: true),
            Showing.Create(
                Showing2Id,
                new DateTime(2025, 6, 1, 13, 0, 0, DateTimeKind.Utc),
                85_000m,
                8,
                MovieSeedData.Movie1Id,
                RoomSeedData.Room1Id,
                TheaterSeedData.Theater1Id,
                timeDuration: 107,
                isActive: true),
            Showing.Create(
                Showing3Id,
                new DateTime(2025, 6, 1, 17, 0, 0, DateTimeKind.Utc),
                90_000m,
                8,
                MovieSeedData.Movie2Id,
                RoomSeedData.Room1Id,
                TheaterSeedData.Theater1Id,
                timeDuration: 112,
                isActive: true),
            // -- Room 2
            Showing.Create(
                Showing4Id,
                new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
                75_000m,
                6,
                MovieSeedData.Movie3Id,
                RoomSeedData.Room2Id,
                TheaterSeedData.Theater1Id,
                timeDuration: 98,
                isActive: true),
            Showing.Create(
                Showing5Id,
                new DateTime(2025, 6, 1, 14, 0, 0, DateTimeKind.Utc),
                100_000m,
                6,
                MovieSeedData.Movie4Id,
                RoomSeedData.Room2Id,
                TheaterSeedData.Theater1Id,
                timeDuration: 145,
                isActive: true),
            // -- Room 3 (IMAX)
            Showing.Create(
                Showing6Id,
                new DateTime(2025, 6, 1, 11, 0, 0, DateTimeKind.Utc),
                120_000m,
                10,
                MovieSeedData.Movie4Id,
                RoomSeedData.Room3Id,
                TheaterSeedData.Theater1Id,
                timeDuration: 145,
                isActive: true),
            Showing.Create(
                Showing7Id,
                new DateTime(2025, 6, 1, 15, 30, 0, DateTimeKind.Utc),
                120_000m,
                10,
                MovieSeedData.Movie7Id,
                RoomSeedData.Room3Id,
                TheaterSeedData.Theater1Id,
                timeDuration: 118,
                isActive: true),
            // -- Room 4 (Theater 2)
            Showing.Create(
                Showing8Id,
                new DateTime(2025, 6, 2,  9, 0, 0, DateTimeKind.Utc),
                80_000m,
                8,
                MovieSeedData.Movie5Id,
                RoomSeedData.Room4Id,
                TheaterSeedData.Theater2Id,
                timeDuration: 95,
                isActive: true),
            Showing.Create(
                Showing9Id,
                new DateTime(2025, 6, 2, 13, 0, 0, DateTimeKind.Utc),
                80_000m,
                8,
                MovieSeedData.Movie6Id,
                RoomSeedData.Room4Id,
                TheaterSeedData.Theater2Id,
                timeDuration: 102,
                isActive: true),
            // -- Room 6 (Theater 3)
            Showing.Create(
                Showing10Id,
                new DateTime(2025, 6, 2, 19, 0, 0, DateTimeKind.Utc),
                90_000m,
                5,
                MovieSeedData.Movie7Id,
                RoomSeedData.Room6Id,
                TheaterSeedData.Theater3Id,
                timeDuration: 118,
                isActive: true)
        ];
    }
}
