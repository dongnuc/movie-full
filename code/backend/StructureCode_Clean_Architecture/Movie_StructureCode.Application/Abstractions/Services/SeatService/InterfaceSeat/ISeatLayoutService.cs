using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat
{
    public interface ISeatLayoutService
    {
        void RecalculateSeatNumbers(List<Seat> rowSeats, Guid coupleSeatTypeId);
    }
}
