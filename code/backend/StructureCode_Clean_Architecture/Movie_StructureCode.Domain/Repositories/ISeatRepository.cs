using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<List<Seat>> GetSeatsWithTypesAsync(List<Guid> seatIds, CancellationToken ct = default);
    }
}
