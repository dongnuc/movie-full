using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;

namespace Movie_StructureCode.Persistence.Repositories
{
    public sealed class ShowingRepository : Repository<Showing>, IShowingRepository
    {
        public ShowingRepository(AppDbContext context) : base(context) { }

        public async Task<PagedResult<Showing>> GetShowingsByRoomAsync(Guid roomId, CancellationToken ct = default)
        {
            var showings = await _context.Showings
                .AsNoTracking()
                .Include(s => s.Movie)
                .Where(s => s.RoomId == roomId)
                .ToListAsync(ct);

            return PagedResult<Showing>.Create(showings, 1, showings.Count, showings.Count);
        }

        public async Task<PagedResult<ShowingGroupedByMovie>> GetAllShowing(
            Guid theaterId,
            DateOnly? dateStart,
            int pageIndex,
            int pageSize,
            bool? isActive,
            CancellationToken ct = default)
        {
            var targetDate = dateStart ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = targetDate.ToDateTime(TimeOnly.MinValue);
            var endDate = targetDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var query = _context.Movies
                .AsNoTracking()
                .Where(m => _context.Showings.Any(s =>
                    s.MovieId == m.Id &&
                    s.TheaterId == theaterId &&
                    s.TimeStart >= startDate &&
                    s.TimeStart < endDate &&
                    (!isActive.HasValue || s.IsActive == isActive.Value)));

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync(ct);
            var movies = await query
                .OrderBy(m => m.Title)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var movieIds = movies.Select(m => m.Id).ToList();

            var showings = await _context.Showings
                .AsNoTracking()
                .Include(s => s.Room)
                .Where(s =>
                    s.TheaterId == theaterId &&
                    movieIds.Contains(s.MovieId) &&
                    s.TimeStart >= startDate &&
                    s.TimeStart < endDate &&
                    (!isActive.HasValue || s.IsActive == isActive.Value))
                .OrderBy(s => s.TimeStart)
                .ToListAsync(ct);

            var items = movies.Select(m => new ShowingGroupedByMovie
            {
                MovieId = m.Id,
                MovieTitle = m.Title,
                Image = m.Image,
                Duration = m.Duration,
                Showings = showings
                .Where(s => s.MovieId == m.Id)
                .Select(s => new ShowingGroupItem
                {
                    Id = s.Id,
                    TimeStart = s.TimeStart,
                    RoomName = s.Room?.Name ?? string.Empty,
                    IsActive = s.IsActive
                })
                .ToList()
            }).ToList();

            return PagedResult<ShowingGroupedByMovie>.Create(items, pageIndex, pageSize, totalCount);
        }

        public async Task<Showing?> GetShowingWithDetailsAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Showings
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }
        public async Task<IEnumerable<Showing>> GetActiveShowingsForMovieTodayAsync(Guid movieId, CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _context.Showings
                .AsNoTracking()
                .Include(s => s.Room)
                    .ThenInclude(r => r.Theater)
                .Where(s =>
                    s.MovieId == movieId &&
                    s.IsActive &&
                    s.Room!.IsActive &&
                    s.Room.Theater!.IsActive &&
                    s.TimeStart >= today &&
                    s.TimeStart < tomorrow)
                .OrderBy(s => s.TimeStart)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Showing>> GetActiveShowingsUserAsync(Guid movieId, Guid theaterId, DateOnly date, CancellationToken ct = default)
        {
            var startDate = date.ToDateTime(TimeOnly.MinValue);
            var endDate = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

            return await _context.Showings
                .AsNoTracking()
                .Include(s => s.Room)
                .Where(s =>
                    s.MovieId == movieId &&
                    s.TheaterId == theaterId &&
                    s.IsActive &&
                    s.Room!.IsActive &&
                    s.TimeStart >= startDate &&
                    s.TimeStart < endDate)
                .OrderBy(s => s.TimeStart)
                .ToListAsync(ct);
        }
    }
}
