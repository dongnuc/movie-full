using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetBookingWithDetailsForEmailAsync(Guid bookingId, CancellationToken ct = default);
        Task<IEnumerable<Booking>> GetBookingHistoryByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Booking?> GetBookingDetailAsync(Guid bookingId, Guid userId, CancellationToken ct = default);
    }
}