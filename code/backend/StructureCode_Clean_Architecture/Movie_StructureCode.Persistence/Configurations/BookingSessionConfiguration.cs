using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.Configurations
{
    public sealed class BookingSessionConfiguration : IEntityTypeConfiguration<BookingSession>
    {
        public void Configure(EntityTypeBuilder<BookingSession> builder)
        {
            builder.ToTable(nameof(BookingSession));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ExpiredAt)
                .IsRequired();

            builder.Property(x => x.Extend)
                .IsRequired();

            builder.Property(x => x.GracePeriodEndsAt)
                .IsRequired();

            builder.Property(x => x.DateCreate)
                .IsRequired();

            builder.Property(x => x.DateUpdate)
                .IsRequired();

            builder.HasOne(x => x.Showing)
                .WithMany()
                .HasForeignKey(x => x.ShowingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AppUser)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
