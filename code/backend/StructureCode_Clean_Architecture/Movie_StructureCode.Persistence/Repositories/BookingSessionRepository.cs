using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class BookingSessionRepository : Repository<BookingSession>, IBookingSessionRepository
    {
        public BookingSessionRepository(AppDbContext context) : base(context) { }

        public async Task<BookingSession?> GetActiveSessionAsync(Guid showingId, Guid userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<BookingSession>()
                .Include(s => s.BookingSessionSeats)
                .FirstOrDefaultAsync(s => 
                    s.ShowingId == showingId && 
                    s.AppUserId == userId && 
                    s.Status != "expired" &&
                    s.CancelledAt == null &&
                    s.GracePeriodEndsAt > now, 
                    ct);
        }

        public async Task<BookingSession?> GetSessionForTimeoutAsync(Guid showingId, Guid userId, CancellationToken ct = default)
        {
            return await _context.Set<BookingSession>()
                .Include(s => s.BookingSessionSeats)
                .FirstOrDefaultAsync(s => 
                    s.ShowingId == showingId && 
                    s.AppUserId == userId && 
                    s.Status != "expired" &&
                    s.CancelledAt == null, 
                    ct);
        }

        public async Task<List<Guid>> GetLockedSeatIdsForShowingAsync(Guid showingId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<BookingSessionSeat>()
                .Include(bss => bss.BookingSession)
                .Where(bss => 
                    bss.BookingSession!.ShowingId == showingId && 
                    bss.BookingSession.Status != "expired" && 
                    bss.BookingSession.CancelledAt == null && 
                    bss.BookingSession.GracePeriodEndsAt > now)
                .Select(bss => bss.SeatId)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task<BookingSession?> GetSessionByShowingAndStatusAsync(Guid showingId, Guid userId, string status, CancellationToken ct = default)
        {
            return await _context.Set<BookingSession>()
                .Include(s => s.BookingSessionSeats)
                .FirstOrDefaultAsync(s => 
                    s.ShowingId == showingId && 
                    s.AppUserId == userId && 
                    s.Status == status &&
                    s.CancelledAt == null, 
                    ct);
        }
    }
}
