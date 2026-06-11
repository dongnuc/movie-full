using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Domain.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetBookingWithDetailsForEmailAsync(Guid bookingId, CancellationToken ct = default);
    }
}