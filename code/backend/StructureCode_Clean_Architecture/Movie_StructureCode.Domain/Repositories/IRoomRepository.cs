using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task CreateRoom(Room room, CancellationToken ct = default);
        Task<PagedResult<Room>> GetAllRoom(string? search, bool? isActive, Guid theaterId, int page, int pageSize, CancellationToken ct = default);
        Task<Room?> GetById(Guid id, CancellationToken ct = default);
        Task<Room?> GetLayoutRoom(Guid roomId, CancellationToken ct = default);
    }
}