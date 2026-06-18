using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Queries.GetBookingHistory
{
    public sealed record BookingHistoryResponse(
        Guid BookingId,
        string MovieTitle,
        string? MovieImage,
        DateTime ShowingStart,
        string RoomName,
        string TheaterName,
        decimal TotalAmount,
        string Status,
        List<string> Seats,
        DateTime DateCreate
    );
}
