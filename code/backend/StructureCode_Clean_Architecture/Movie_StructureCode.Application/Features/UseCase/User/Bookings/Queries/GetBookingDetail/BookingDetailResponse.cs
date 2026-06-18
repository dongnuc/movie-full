using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Queries.GetBookingDetail
{
    public sealed record BookingDetailResponse(
        Guid BookingId,
        string MovieTitle,
        string? MovieImage,
        DateTime ShowingStart,
        string RoomName,
        string TheaterName,
        string TheaterLocation,
        decimal TotalAmount,
        string Status,
        List<BookingDetailItemResponse> Details,
        DateTime DateCreate
    );

    public sealed record BookingDetailItemResponse(
        Guid BookingDetailId,
        string SeatRow,
        int SeatNumber,
        string SeatType,
        decimal Price
    );
}
