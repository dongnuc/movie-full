using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IShowingSeatRepository : IRepository<ShowingSeat>
    {
        Task<List<ShowingSeat>> GetShowingSeatsWithBooking(Guid showingId, CancellationToken ct = default);
    }
}
