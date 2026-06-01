using System.Collections.Generic;
using System.Linq;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Mappers
{
    public static class RoomMapper
    {
        public static RoomResponse ToResponse(Room room)
        {
            return new RoomResponse(
                room.Id,
                room.Name,
                room.TotalSeat,
                $"{room.TotalRow}x{room.TotalCol}",
                room.IsActive,
                room.DateCreate,
                room.DateUpdate);
        }

        public static RoomDetailResponse ToDetailResponse(Room room)
        {
            return new RoomDetailResponse(
                room.Id,
                room.Name,
                room.TotalSeat,
                room.TotalRow,
                room.TotalCol,
                room.IsActive,
                room.DateCreate,
                room.DateUpdate);
        }

        public static RoomLayoutResponse ToLayoutResponse(Room room)
        {
            if (room?.Seats == null || !room.Seats.Any())
            {
                return new RoomLayoutResponse(
                    room.Id,
                    room.Name,
                    room.TotalSeat,
                    room.TotalRow,
                    room.TotalCol,
                    room.IsActive,
                    new List<SeatRowLayoutResponse>());
            }

            var seatLayouts = room.Seats.GroupBy(s => s.RowIndex).Select(group => new SeatRowLayoutResponse(
                    group.Key,
                    group.First().RowName,
                    group.Select(s => new SeatLayoutResponse(
                        s.Id,
                        s.ColIndex,
                        s.SeatNumber,
                        s.GroupId,
                        s.SeatTypeId,
                        s.SeatType?.Name ?? string.Empty,
                        s.SeatType?.Surcharge ?? 0,
                        s.IsActive))
                    .OrderBy(s => s.ColIndex)
                    .ToList()
                ))
                .OrderBy(group => group.RowIndex)
                .ToList();

            return new RoomLayoutResponse(
                room.Id,
                room.Name,
                room.TotalSeat,
                room.TotalRow,
                room.TotalCol,
                room.IsActive,
                seatLayouts);
        }
    }
}
