using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomLayout;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetShowingSeatsLayout
{
    internal sealed class GetShowingSeatsLayoutQueryHandler : IQueryHandler<GetShowingSeatsLayoutQuery, UserRoomLayoutResponse>
    {
        private readonly ISender _sender;
        private readonly ISeatLockService _seatLockService;
        private readonly IShowingRepository _showingRepository;

        public GetShowingSeatsLayoutQueryHandler(
            ISender sender,
            ISeatLockService seatLockService,
            IShowingRepository showingRepository)
        {
            _sender = sender;
            _seatLockService = seatLockService;
            _showingRepository = showingRepository;
        }

        public async Task<Result<UserRoomLayoutResponse>> Handle(
            GetShowingSeatsLayoutQuery request,
            CancellationToken cancellationToken)
        {
            var showing = await _showingRepository.GetShowingWithDetailsAsync(request.ShowingId, cancellationToken);
            if (showing == null)
            {
                return Result.Failure<UserRoomLayoutResponse>(new Error("Showing.NotFound", "Không tìm thấy suất chiếu."));
            }

            var roomId = request.RoomId ?? showing.RoomId;

            var roomLayoutResult = await _sender.Send(new GetRoomLayoutQuery(roomId), cancellationToken);
            if (roomLayoutResult.IsFailure)
            {
                return Result.Failure<UserRoomLayoutResponse>(roomLayoutResult.Error);
            }
            var roomLayout = roomLayoutResult.Value;
            if (roomLayout == null)
            {
                return Result.Failure<UserRoomLayoutResponse>(new Error("Layout.NotFound", "Không tìm thấy sơ đồ phòng chiếu."));
            }

            // Fetch seat states directly from ISeatLockService (which wraps Redis and DB fallback logic internally)
            var bookedSeatIds = await _seatLockService.GetBookedSeatsAsync(request.ShowingId, cancellationToken);
            Console.WriteLine($"Booked Seat IDs: {string.Join(", ", bookedSeatIds)}");
            var holdingSeatIds = (await _seatLockService.GetAllLockedSeatsByShowingAsync(request.ShowingId)).ToHashSet();
            Console.WriteLine($"Holding Seat IDs: {string.Join(", ", holdingSeatIds)}");
            
            var myHoldingSeatIds = request.CurrentUserId.HasValue
                ? await _seatLockService.GetUserHoldingSeatsAsync(request.ShowingId, request.CurrentUserId.Value.ToString())
                : new HashSet<Guid>();

            // Ensure consistency: any seat held by the current user is also counted as holding
            holdingSeatIds.UnionWith(myHoldingSeatIds);

            var seatRowLayouts = roomLayout.SeatRows.Select(row =>
            {
                var seatsInRow = new List<UserSeatLayoutResponse>();

                foreach (var seatLayout in row.Seats)
                {
                    bool isBooked = false;
                    bool isLocked = false;
                    bool isMine = false;

                    foreach (var seatId in seatLayout.SeatIds.SeatIds)
                    {
                        if (bookedSeatIds.Contains(seatId))
                        {
                            isBooked = true;
                        }
                        else if (holdingSeatIds.Contains(seatId))
                        {
                            isLocked = true;
                            if (myHoldingSeatIds.Contains(seatId))
                            {
                                isMine = true;
                            }
                        }
                    }

                    string status = "Available";
                    if (isBooked)
                        status = "Booked";
                    else if (isLocked)
                        status = "Holding";

                    seatsInRow.Add(new UserSeatLayoutResponse(
                        seatLayout.ColIndex,
                        seatLayout.SeatNumber,
                        seatLayout.GroupId,
                        seatLayout.SeatTypeId,
                        seatLayout.SeatTypeName,
                        seatLayout.Surcharge,
                        new UserSeatGroupResponse(seatLayout.SeatIds.SeatIds),
                        seatLayout.IsActive,
                        status,
                        isMine));
                }

                return new UserSeatRowLayoutResponse(
                    row.RowIndex,
                    row.RowName,
                    seatsInRow.OrderBy(s => s.ColIndex).ToList());
            })
            .OrderBy(group => group.RowIndex)
            .ToList();

            var response = new UserRoomLayoutResponse(
                roomLayout.Id,
                roomLayout.Name,
                seatRowLayouts,
                showing.MovieId,
                showing.Movie?.Title ?? string.Empty,
                showing.Price,
                roomId,
                showing.Id);

            return Result.Success(response);
        }
    }
}
