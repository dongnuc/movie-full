using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IShowingRepository : IRepository<Showing>
    {
        Task<PagedResult<Showing>> GetShowingsByRoomAsync(Guid roomId, CancellationToken ct = default);
        Task<PagedResult<ShowingGroupedByMovie>> GetAllShowing(
            Guid theaterId,
            DateOnly? dateStart,
            Guid? roomId,
            int pageIndex,
            int pageSize,
            bool? isActive,
            CancellationToken ct = default);

        Task<PagedResult<ShowingGroupedRoom>> GetAllShowingGroupedRoom(
            Guid theaterId,
            DateOnly? dateStart,
            Guid? roomId,
            int pageIndex,
            int pageSize,
            bool? isActive,
            CancellationToken ct = default);
        
        Task<Showing?> GetShowingWithDetailsAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Showing>> GetActiveShowingsForMovieTodayAsync(Guid movieId, CancellationToken ct = default);
        Task<IEnumerable<Showing>> GetActiveShowingsUserAsync(Guid movieId, Guid theaterId, DateOnly date, CancellationToken ct = default);
    }
}