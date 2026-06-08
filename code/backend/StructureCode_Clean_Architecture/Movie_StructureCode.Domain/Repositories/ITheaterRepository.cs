using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface ITheaterRepository : IRepository<Theater>
    {
        Task<PagedResult<Theater>> GetTheatersPagedAsync(
            string? search,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<System.Collections.Generic.List<(Theater Theater, int ShowingCount)>> GetActiveTheatersWithShowingCountAsync(
            System.Guid movieId,
            System.DateTime startDate,
            System.DateTime endDate,
            System.Threading.CancellationToken ct = default);
    }
}
