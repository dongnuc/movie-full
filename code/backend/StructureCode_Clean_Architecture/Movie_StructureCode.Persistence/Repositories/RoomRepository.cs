using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(AppDbContext context) : base(context) { }

        public Task CreateRoom(Room room, CancellationToken ct = default)
        {
            return AddAsync(room, ct);
        }

        public async Task<PagedResult<Room>> GetAllRoom(string? search, bool? isActive, Guid theaterId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Rooms
                .Include(r => r.Theater)
                .AsNoTracking()
                .Where(r => r.TheaterId == theaterId)
                .OrderBy(r => r.Name)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Name.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            return await PagedResult<Room>.CreateAsync(query, page, pageSize);
        }

        public Task<Room?> GetById(Guid id, CancellationToken ct = default)
        {
            return _context.Rooms
                .Include(r => r.Theater)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public Task<Room?> GetLayoutRoom(Guid roomId, CancellationToken ct = default)
        {
            return _context.Rooms
                .Include(r => r.Seats!)
                    .ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(r => r.Id == roomId, ct);
        }
    }
}
