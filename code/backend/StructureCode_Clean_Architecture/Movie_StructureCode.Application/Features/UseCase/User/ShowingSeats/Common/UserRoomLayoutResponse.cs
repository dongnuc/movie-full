using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Common
{
    public sealed record UserSeatGroupResponse(
        List<Guid> SeatIds
    );

    public sealed record UserSeatLayoutResponse(
        int ColIndex,
        int SeatNumber,
        Guid? GroupId,
        Guid SeatTypeId,
        string SeatTypeName,
        decimal Surcharge,
        UserSeatGroupResponse SeatIds,
        bool IsActive,
        string Status, // "Available", "Holding", "Booked"
        bool IsMine
    );

    public sealed record UserSeatRowLayoutResponse(
        int RowIndex,
        string RowName,
        List<UserSeatLayoutResponse> Seats
    );

    public sealed record UserRoomLayoutResponse(
        Guid Id,
        string Name,
        List<UserSeatRowLayoutResponse> SeatRows,
        Guid MovieId,
        string MovieTitle,
        decimal BasePrice,
        Guid RoomId,
        Guid ShowingId
    );
}
