using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface ISeatTypeRepository : IRepository<SeatType>
    {
        Task<PagedResult<SeatType>> GetAllAsync(Guid? id, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<SeatType>> GetActiveSeatTypesAsync(CancellationToken ct = default);
    }
}
