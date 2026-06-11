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
    public sealed class ShowingSeatRepository : Repository<ShowingSeat>, IShowingSeatRepository
    {
        private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(10);

        public ShowingSeatRepository(AppDbContext context) : base(context) { }

        public Task<List<ShowingSeat>> GetShowingSeatsWithBooking(Guid showingId, CancellationToken ct = default)
        {
            return _context.ShowingSeats
                .Include(ss => ss.Booking)
                .Where(ss => ss.ShowingId == showingId)
                .ToListAsync(ct);
        }

        public Task<List<ShowingSeat>> GetShowingSeatsForBooking(Guid showingId, List<Guid> seatIds, CancellationToken ct = default)
        {
            return _context.ShowingSeats
                .Where(ss => ss.ShowingId == showingId && seatIds.Contains(ss.SeatId))
                .ToListAsync(ct);
        }
    }
}
