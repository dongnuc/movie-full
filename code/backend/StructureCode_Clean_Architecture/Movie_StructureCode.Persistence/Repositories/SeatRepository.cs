using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(AppDbContext context) : base(context) { }

        public Task<List<Seat>> GetSeatsWithTypesAsync(List<Guid> seatIds, CancellationToken ct = default)
        {
            return _context.Seats
                .Include(s => s.SeatType)
                .Where(s => seatIds.Contains(s.Id))
                .ToListAsync(ct);
        }
    }
}
