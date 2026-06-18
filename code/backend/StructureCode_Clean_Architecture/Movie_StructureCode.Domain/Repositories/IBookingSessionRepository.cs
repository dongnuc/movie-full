using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IBookingSessionRepository : IRepository<BookingSession>
    {
        Task<BookingSession?> GetActiveSessionAsync(Guid showingId, Guid userId, CancellationToken ct = default);
        Task<BookingSession?> GetSessionForTimeoutAsync(Guid showingId, Guid userId, CancellationToken ct = default);
        Task<List<Guid>> GetLockedSeatIdsForShowingAsync(Guid showingId, CancellationToken ct = default);
        Task<BookingSession?> GetSessionByShowingAndStatusAsync(Guid showingId, Guid userId, string status, CancellationToken ct = default);
    }
}
