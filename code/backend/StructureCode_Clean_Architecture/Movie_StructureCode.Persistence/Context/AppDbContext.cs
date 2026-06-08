
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Persistence.SeedData;

namespace Movie_StructureCode.Persistence.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<SeatType> SeatTypes { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Showing> Showings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<ShowingSeat> ShowingSeats { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Theater_User> TheaterUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);

            // Set all DateTime properties to UTC kind
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                            v => !v.HasValue ? v : (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)),
                            v => !v.HasValue ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)));
                    }
                }
            }

            //builder.Entity<Category>().HasData(CategorySeedData.GetSeedData());
            //builder.Entity<Movie>().HasData(MovieSeedData.GetSeedData());
            //builder.Entity<Theater>().HasData(TheaterSeedData.GetSeedData());
            //builder.Entity<Room>().HasData(RoomSeedData.GetSeedData());
            //builder.Entity<SeatType>().HasData(SeatTypeSeedData.GetSeedData());
            //builder.Entity<Seat>().HasData(SeatSeedData.GetSeedData());
            //builder.Entity<Showing>().HasData(ShowingSeedData.GetSeedData());
        }
    }
}
