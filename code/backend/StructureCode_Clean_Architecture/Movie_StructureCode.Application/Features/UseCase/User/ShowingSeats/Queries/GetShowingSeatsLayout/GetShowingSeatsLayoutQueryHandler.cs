using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetShowingSeatsLayout
{
    internal sealed class GetShowingSeatsLayoutQueryHandler : IQueryHandler<GetShowingSeatsLayoutQuery, UserRoomLayoutResponse>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IShowingSeatRepository _showingSeatRepository;

        public GetShowingSeatsLayoutQueryHandler(
            IRoomRepository roomRepository,
            IShowingSeatRepository showingSeatRepository)
        {
            _roomRepository = roomRepository;
            _showingSeatRepository = showingSeatRepository;
        }

        public async Task<Result<UserRoomLayoutResponse>> Handle(
            GetShowingSeatsLayoutQuery request,
            CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetLayoutRoom(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<UserRoomLayoutResponse>(RoomErrors.NotFound(request.RoomId));
            }

            var showingSeats = await _showingSeatRepository.GetShowingSeatsWithBooking(request.ShowingId, cancellationToken);
            var showingSeatMap = showingSeats.ToDictionary(ss => ss.SeatId);

            var seatRowLayouts = room.Seats.GroupBy(s => s.RowIndex).Select(group =>
            {
                var seatsInRow = new List<UserSeatLayoutResponse>();

                // Nhóm các ghế couple có chung GroupId trong cùng một hàng thành 1 ghế đại diện nhưng giữ tất cả SeatIds
                var coupleSeats = group
                    .Where(s => s.SeatType?.Name?.Equals("couple", StringComparison.OrdinalIgnoreCase) == true && s.GroupId.HasValue)
                    .GroupBy(s => s.GroupId!.Value)
                    .Select(g =>
                    {
                        var sorted = g.OrderBy(s => s.ColIndex).ToList();
                        var first = sorted.First();
                        var allIds = sorted.Select(s => s.Id).ToList();

                        // Xác định trạng thái và quyền sở hữu (isMine)
                        bool isBooked = false;
                        bool isLocked = false;
                        bool isMine = false;

                        foreach (var seatId in allIds)
                        {
                            if (showingSeatMap.TryGetValue(seatId, out var ss))
                            {
                                if (ss.Status == ShowingSeatStatus.Booked)
                                    isBooked = true;
                                else if (ss.Status == ShowingSeatStatus.Locked)
                                    isLocked = true;

                                if (request.CurrentUserId.HasValue && ss.Booking != null && ss.Booking.AppUserId == request.CurrentUserId.Value)
                                    isMine = true;
                            }
                        }

                        string status = "Available";
                        if (isBooked)
                            status = "Booked";
                        else if (isLocked)
                            status = "Holding";

                        return new UserSeatLayoutResponse(
                            first.ColIndex,
                            first.SeatNumber,
                            first.GroupId,
                            first.SeatTypeId,
                            first.SeatType?.Name ?? string.Empty,
                            first.SeatType?.Surcharge ?? 0,
                            new UserSeatGroupResponse(allIds),
                            first.IsActive,
                            status,
                            isMine);
                    });

                // Các ghế khác (không phải couple hoặc không có GroupId) giữ nguyên nhưng bọc ID trong SeatGroupResponse
                var otherSeats = group
                    .Where(s => !(s.SeatType?.Name?.Equals("couple", StringComparison.OrdinalIgnoreCase) == true && s.GroupId.HasValue))
                    .Select(s =>
                    {
                        // Xác định trạng thái và quyền sở hữu (isMine)
                        bool isBooked = false;
                        bool isLocked = false;
                        bool isMine = false;

                        if (showingSeatMap.TryGetValue(s.Id, out var ss))
                        {
                            if (ss.Status == ShowingSeatStatus.Booked)
                                isBooked = true;
                            else if (ss.Status == ShowingSeatStatus.Locked)
                                isLocked = true;

                            if (request.CurrentUserId.HasValue && ss.Booking != null && ss.Booking.AppUserId == request.CurrentUserId.Value)
                                isMine = true;
                        }

                        string status = "Available";
                        if (isBooked)
                            status = "Booked";
                        else if (isLocked)
                            status = "Holding";

                        return new UserSeatLayoutResponse(
                            s.ColIndex,
                            s.SeatNumber,
                            s.GroupId,
                            s.SeatTypeId,
                            s.SeatType?.Name ?? string.Empty,
                            s.SeatType?.Surcharge ?? 0,
                            new UserSeatGroupResponse(new List<Guid> { s.Id }),
                            s.IsActive,
                            status,
                            isMine);
                    });

                seatsInRow.AddRange(coupleSeats);
                seatsInRow.AddRange(otherSeats);

                return new UserSeatRowLayoutResponse(
                    group.Key,
                    group.First().RowName,
                    seatsInRow.OrderBy(s => s.ColIndex).ToList());
            })
            .OrderBy(group => group.RowIndex)
            .ToList();

            var response = new UserRoomLayoutResponse(
                room.Id,
                room.Name,
                seatRowLayouts);

            return Result.Success(response);
        }
    }
}
