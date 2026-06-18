using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Queries.GetBookingDetail
{
    public sealed record GetBookingDetailQuery(Guid BookingId, Guid UserId) : IQuery<BookingDetailResponse>;
}
