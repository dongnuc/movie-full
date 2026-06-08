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

        public async Task<System.Collections.Generic.List<Movie>> GetMoviesFeaturedTodayAsync(System.DateTime startDate, System.DateTime endDate, System.Threading.CancellationToken ct = default)
        {
            return await _context.Movies
                .AsNoTracking()
                .Include(m => m.Category)
                .Where(m => m.IsActive)
                .Select(m => new
                {
                    Movie = m,
                    ShowingCount = _context.Showings.Count(s =>
                        s.MovieId == m.Id &&
                        s.IsActive &&
                        s.TimeStart >= startDate &&
                        s.TimeStart < endDate)
                })
                .Where(x => x.ShowingCount > 0)
                .OrderByDescending(x => x.ShowingCount)
                .ThenBy(x => x.Movie.Title)
                .Take(12)
                .Select(x => x.Movie)
                .ToListAsync(ct);
        }

        public async Task<System.Collections.Generic.List<Movie>> GetActiveMoviesWithoutShowingsAsync(int limit, System.Threading.CancellationToken ct = default)
        {
            return await _context.Movies
                .AsNoTracking()
                .Include(m => m.Category)
                .Where(m => m.IsActive && !_context.Showings.Any(s => s.MovieId == m.Id))
                .OrderBy(m => m.Title)
                .Take(limit)
                .ToListAsync(ct);
        }
    }
}

