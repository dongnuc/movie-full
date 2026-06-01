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
    }
}
