using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.Configurations
{
    public sealed class ShowingConfiguration : IEntityTypeConfiguration<Showing>
    {
        public void Configure(EntityTypeBuilder<Showing> builder)
        {
            builder.ToTable(nameof(Showing));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.TimeStart)
                .IsRequired();

            builder.Property(x => x.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.MaxTicketsPerUser)
                .IsRequired();

            builder.Property(x => x.DateCreate)
                .IsRequired();

            builder.Property(x => x.DateUpdate)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.TheaterId)
                .IsRequired();

            builder.HasOne(x => x.Movie)
                .WithMany(s => s.Showings)
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Room)
                .WithMany(s => s.Showings)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // ?? INDEX ????????????????????????????????????????????????????????
            // Composite index: (TheaterId, IsActive, MovieId) - t?i ?u query GetMoviesByTheaterId
            builder.HasIndex(x => new { x.TheaterId, x.IsActive, x.MovieId })
                .HasDatabaseName("IX_Showing_TheaterId_IsActive_MovieId")
                .IsUnique(false);

            // Single index: TheaterId - t?i ?u query l?c theo theater
            builder.HasIndex(x => x.TheaterId)
                .HasDatabaseName("IX_Showing_TheaterId")
                .IsUnique(false);

            // Composite index: (MovieId, TheaterId, IsActive) - t?i ?u filter movies by theater
            builder.HasIndex(x => new { x.MovieId, x.TheaterId, x.IsActive })
                .HasDatabaseName("IX_Showing_MovieId_TheaterId_IsActive")
                .IsUnique(false);
        }
    }
}
