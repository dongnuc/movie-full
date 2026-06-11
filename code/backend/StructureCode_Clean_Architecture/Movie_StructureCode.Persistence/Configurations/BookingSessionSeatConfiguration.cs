using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.Configurations
{
    public sealed class BookingSessionSeatConfiguration : IEntityTypeConfiguration<BookingSessionSeat>
    {
        public void Configure(EntityTypeBuilder<BookingSessionSeat> builder)
        {
            builder.ToTable(nameof(BookingSessionSeat));
            
            // Composite key
            builder.HasKey(x => new { x.BookingSessionId, x.SeatId });

            builder.HasOne(x => x.BookingSession)
                .WithMany(s => s.BookingSessionSeats)
                .HasForeignKey(x => x.BookingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Seat)
                .WithMany()
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
