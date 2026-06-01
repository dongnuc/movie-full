using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class MovieRepository : Repository<Movie>, IMovieRepository
    {
        public MovieRepository(AppDbContext context) : base(context) { }

        public async Task<Movie?> GetByIdWithCategoryAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Movies
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task<PagedResult<Movie>> GetMoviesPagedAsync(
            string? search,
            Guid? categoryId,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.Movies
                .Include(m => m.Category)
                .AsNoTracking()
                .OrderBy(m => m.Title)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.Contains(search) || (m.Description != null && m.Description.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            return await PagedResult<Movie>.CreateAsync(query, page, pageSize);
        }

        public async Task<Movie?> GetActiveMovieWithCategoryAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Movies
                .AsNoTracking()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, ct);
        }
    }
}
