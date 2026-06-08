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
            if (room == null || room.Seats == null || !room.Seats.Any())
            {
                return new RoomLayoutResponse(
                    room?.Id ?? Guid.Empty,
                    room?.Name ?? string.Empty,
                    room?.TotalSeat ?? 0,
                    room?.TotalRow ?? 0,
                    room?.TotalCol ?? 0,
                    room?.IsActive ?? false,
                    new List<SeatRowLayoutResponse>());
            }

            var seatLayouts = room.Seats.GroupBy(s => s.RowIndex).Select(group =>
            {
                var seatsInRow = new List<SeatLayoutResponse>();

                // Nhóm các ghế couple có chung GroupId trong cùng một hàng thành 1 ghế đại diện nhưng giữ tất cả SeatIds
                var coupleSeats = group
                    .Where(s => s.SeatType?.Name?.Equals("couple", System.StringComparison.OrdinalIgnoreCase) == true && s.GroupId.HasValue)
                    .GroupBy(s => s.GroupId!.Value)
                    .Select(g =>
                    {
                        var sorted = g.OrderBy(s => s.ColIndex).ToList();
                        var first = sorted.First();
                        var allIds = sorted.Select(s => s.Id).ToList();
                        return new SeatLayoutResponse(
                            first.ColIndex,
                            first.SeatNumber,
                            first.GroupId,
                            first.SeatTypeId,
                            first.SeatType?.Name ?? string.Empty,
                            first.SeatType?.Surcharge ?? 0,
                            new SeatGroupResponse(allIds),
                            first.IsActive);
                    });

                // Các ghế khác (không phải couple hoặc không có GroupId) giữ nguyên nhưng bọc ID trong SeatGroupResponse
                var otherSeats = group
                    .Where(s => !(s.SeatType?.Name?.Equals("couple", System.StringComparison.OrdinalIgnoreCase) == true && s.GroupId.HasValue))
                    .Select(s => new SeatLayoutResponse(
                        s.ColIndex,
                        s.SeatNumber,
                        s.GroupId,
                        s.SeatTypeId,
                        s.SeatType?.Name ?? string.Empty,
                        s.SeatType?.Surcharge ?? 0,
                        new SeatGroupResponse(new List<Guid> { s.Id }),
                        s.IsActive));

                seatsInRow.AddRange(coupleSeats);
                seatsInRow.AddRange(otherSeats);

                return new SeatRowLayoutResponse(
                    group.Key,
                    group.First().RowName,
                    seatsInRow.OrderBy(s => s.ColIndex).ToList());
            })
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
