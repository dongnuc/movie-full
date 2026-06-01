using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common
{
    public sealed record SeatLayoutResponse(
        Guid Id,
        int ColIndex,
        int SeatNumber,
        Guid? GroupId,
        Guid SeatTypeId,
        string SeatTypeName,
        decimal Surcharge,
        bool IsActive);

    public sealed record SeatRowLayoutResponse(
        int RowIndex,
        string RowName,
        List<SeatLayoutResponse> Seats
    );

    public sealed record RoomLayoutResponse(
        Guid Id,
        string Name,
        int TotalSeat,
        int TotalRow,
        int TotalCol,
        bool IsActive,
        List<SeatRowLayoutResponse> SeatRows);
}
