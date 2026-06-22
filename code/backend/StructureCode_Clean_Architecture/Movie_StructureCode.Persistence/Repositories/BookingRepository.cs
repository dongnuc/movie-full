using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<Booking>> GetBookingHistoryByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Bookings
                .Include(b => b.Showing)
                    .ThenInclude(s => s!.Movie)
                .Include(b => b.Showing)
                    .ThenInclude(s => s!.Room)
                        .ThenInclude(r => r!.Theater)
                .Include(b => b.BookingDetails!)
                    .ThenInclude(bd => bd.ShowingSeat)
                        .ThenInclude(ss => ss!.Seat)
                .Where(b => b.AppUserId == userId)
                .OrderByDescending(b => b.DateCreate)
                .ToListAsync(ct);
        }

        public Task<Booking?> GetBookingDetailAsync(Guid bookingId, Guid userId, CancellationToken ct = default)
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
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.AppUserId == userId, ct);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByShowingIdAsync(Guid showingId, CancellationToken ct = default)
        {
            return await _context.Bookings
                .Include(b => b.AppUser)
                .Include(b => b.BookingDetails!)
                    .ThenInclude(bd => bd.ShowingSeat)
                        .ThenInclude(ss => ss!.Seat)
                .Where(b => b.ShowingId == showingId)
                .OrderByDescending(b => b.DateCreate)
                .ToListAsync(ct);
        }
    }
}
