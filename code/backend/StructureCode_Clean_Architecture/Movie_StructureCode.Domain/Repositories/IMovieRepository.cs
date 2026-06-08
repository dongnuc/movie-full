using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<Movie?> GetByIdWithCategoryAsync(Guid id, CancellationToken ct = default);

        Task<PagedResult<Movie>> GetMoviesPagedAsync(
            string? search,
            Guid? categoryId,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<Movie?> GetActiveMovieWithCategoryAsync(Guid id, CancellationToken ct = default);

        Task<List<Movie>> GetMoviesFeaturedTodayAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

        Task<List<Movie>> GetActiveMoviesWithoutShowingsAsync(int limit, CancellationToken ct = default);
    }
}

