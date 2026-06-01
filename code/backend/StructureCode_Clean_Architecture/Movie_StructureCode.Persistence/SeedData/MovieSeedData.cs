using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.SeedData
{
    public static class MovieSeedData
    {
        public static readonly Guid Movie1Id = new("22222222-0000-0000-0000-000000000001");
        public static readonly Guid Movie2Id = new("22222222-0000-0000-0000-000000000002");
        public static readonly Guid Movie3Id = new("22222222-0000-0000-0000-000000000003");
        public static readonly Guid Movie4Id = new("22222222-0000-0000-0000-000000000004");
        public static readonly Guid Movie5Id = new("22222222-0000-0000-0000-000000000005");
        public static readonly Guid Movie6Id = new("22222222-0000-0000-0000-000000000006");
        public static readonly Guid Movie7Id = new("22222222-0000-0000-0000-000000000007");

        public static IEnumerable<Movie> GetSeedData() =>
        [
            Movie.Create(
                Movie1Id,
                "The Last Stand",
                "An action-packed movie about a retired sheriff who must stop a drug cartel leader.",
                "the_last_stand.jpg",
                85_000m,
                107,
                CategorySeedData.ActionId,
                isActive: true
            ),
            Movie.Create(
                Movie2Id,
                "Midnight Fear",
                "A group of friends encounter a supernatural force in an abandoned hospital.",
                "midnight_fear.jpg",
                90_000m,
                112,
                CategorySeedData.HorrorId,
                isActive: true
            ),
            Movie.Create(
                Movie3Id,
                "Love in the City",
                "A heartwarming romantic comedy set in the streets of Hanoi.",
                "love_in_the_city.jpg",
                75_000m,
                98,
                CategorySeedData.RomanceId,
                isActive: true
            ),
            Movie.Create(
                Movie4Id,
                "Galactic Odyssey",
                "Astronauts embark on a dangerous mission to the edge of the known universe.",
                "galactic_odyssey.jpg",
                100_000m,
                145,
                CategorySeedData.SciFiId,
                isActive: true
            ),
            Movie.Create(
                Movie5Id,
                "Funny Guys",
                "Two unlikely friends stumble into a series of hilarious misadventures.",
                "funny_guys.jpg",
                70_000m,
                95,
                CategorySeedData.ComedyId,
                isActive: true
            ),
            Movie.Create(
                Movie6Id,
                "Dragon Kingdom",
                "An animated adventure following a young hero destined to save the dragon realm.",
                "dragon_kingdom.jpg",
                80_000m,
                102,
                CategorySeedData.AnimationId,
                isActive: true
            ),
            Movie.Create(
                Movie7Id,
                "Silent Witness",
                "A detective races against time to uncover a serial killer before the next strike.",
                "silent_witness.jpg",
                90_000m,
                118,
                CategorySeedData.ThrillerID,
                isActive: true
            )
        ];
    }
}
