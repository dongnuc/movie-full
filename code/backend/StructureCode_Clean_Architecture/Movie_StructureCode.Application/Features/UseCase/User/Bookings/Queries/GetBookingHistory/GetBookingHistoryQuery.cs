using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Queries.GetBookingHistory
{
    public sealed record GetBookingHistoryQuery(Guid UserId) : IQuery<List<BookingHistoryResponse>>;
}
