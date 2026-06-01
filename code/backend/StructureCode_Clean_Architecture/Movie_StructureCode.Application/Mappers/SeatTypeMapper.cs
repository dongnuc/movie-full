using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using SeatType = Movie_StructureCode.Domain.Entities.SeatType;

namespace Movie_StructureCode.Application.Mappers
{
    public static class SeatTypeMapper
    {
        public static SeatTypeResponse ToResponse(SeatType seatType)
        {
            return new SeatTypeResponse(
                seatType.Id,
                seatType.Name,
                seatType.Surcharge,
                seatType.IsActive,
                seatType.DateCreate,
                seatType.DateUpdate);
        }

        public static SeatTypeActiveResponse ToActiveResponse(SeatType seatType)
        {
            return new SeatTypeActiveResponse(
                seatType.Id,
                seatType.Name,
                seatType.Surcharge);
        }
    }
}
