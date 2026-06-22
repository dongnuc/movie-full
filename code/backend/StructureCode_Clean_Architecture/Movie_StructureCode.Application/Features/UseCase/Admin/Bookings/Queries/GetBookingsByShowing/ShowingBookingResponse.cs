using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Bookings.Queries.GetBookingsByShowing
{
    public sealed record ShowingBookingResponse(
        Guid BookingId,
        string CustomerName,
        string CustomerEmail,
        decimal TotalAmount,
        string Status,
        List<string> Seats,
        DateTime DateCreate
    );
}
