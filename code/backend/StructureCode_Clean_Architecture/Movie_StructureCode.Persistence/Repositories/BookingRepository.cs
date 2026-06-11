using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context) { }

        public Task<Booking?> GetBookingWithDetailsForEmailAsync(Guid bookingId, CancellationToken ct = default)
        {
            return _context.Bookings
                .Include(b => b.AppUser)
                .Include(b => b.Showing)
                    .ThenInclude(s => s!.Movie)
                .Include(b => b.Showing)
                    .ThenInclude(s => s!.Room)
                        .ThenInclude(r => r!.Theater)
                .Include(b => b.BookingDetails!)
                    .ThenInclude(bd => bd.ShowingSeat)
                        .ThenInclude(ss => ss!.Seat)
                            .ThenInclude(s => s!.SeatType)
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        }
    }
}
