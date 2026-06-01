using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.Configurations
{
    public sealed class ShowingSeatConfiguration : IEntityTypeConfiguration<ShowingSeat>
    {
        public void Configure(EntityTypeBuilder<ShowingSeat> builder)
        {
            builder.ToTable(nameof(ShowingSeat));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(ShowingSeatStatus.Available);

            builder.Property(x => x.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.LockedUntil)
                .IsRequired(false);

            builder.Property(x => x.DateCreate)
                .IsRequired();

            builder.Property(x => x.DateUpdate)
                .IsRequired();

            // ── Unique: mỗi ghế chỉ xuất hiện 1 lần trong 1 suất chiếu ─────
            builder.HasIndex(x => new { x.ShowingId, x.SeatId })
                .IsUnique();

            builder.HasOne(x => x.Showing)
                .WithMany()
                .HasForeignKey(x => x.ShowingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Seat)
                .WithMany()
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
