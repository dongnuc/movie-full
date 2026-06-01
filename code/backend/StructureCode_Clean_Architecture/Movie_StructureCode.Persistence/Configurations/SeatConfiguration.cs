using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.Configurations
{
    public sealed class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.ToTable(nameof(Seat));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.RowIndex)
                .IsRequired();

            builder.Property(x => x.ColIndex)
                .IsRequired();

            builder.Property(x => x.RowName)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.SeatNumber)
                .IsRequired();

            builder.Property(x => x.DateCreate)
                .IsRequired();

            builder.Property(x => x.DateUpdate)
                .IsRequired();

            builder.HasOne(x => x.SeatType)
                .WithMany(s => s.Seats)
                .HasForeignKey(x => x.SeatTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Room)
                .WithMany(s => s.Seats)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
