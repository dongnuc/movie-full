using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class TheaterRepository : Repository<Theater>, ITheaterRepository
    {
        public TheaterRepository(AppDbContext context) : base(context) { }

        public async Task<PagedResult<Theater>> GetTheatersPagedAsync(
            string? search,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.Theaters
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Name.Contains(search) || (t.Location != null && t.Location.Contains(search)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(t => t.IsActive == isActive.Value);
            }

            return await PagedResult<Theater>.CreateAsync(query, page, pageSize);
        }

        public async Task<System.Collections.Generic.List<(Theater Theater, int ShowingCount)>> GetActiveTheatersWithShowingCountAsync(
            System.Guid movieId,
            System.DateTime startDate,
            System.DateTime endDate,
            System.Threading.CancellationToken ct = default)
        {
            var result = await _context.Theaters
                .AsNoTracking()
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    Theater = t,
                    ShowingCount = _context.Showings.Count(s =>
                        s.TheaterId == t.Id &&
                        s.MovieId == movieId &&
                        s.IsActive &&
                        s.TimeStart >= startDate &&
                        s.TimeStart < endDate)
                })
                .Where(x => x.ShowingCount > 0)
                .OrderBy(x => x.Theater.Name)
                .ToListAsync(ct);

            return result.Select(x => (x.Theater, x.ShowingCount)).ToList();
        }
    }
}
