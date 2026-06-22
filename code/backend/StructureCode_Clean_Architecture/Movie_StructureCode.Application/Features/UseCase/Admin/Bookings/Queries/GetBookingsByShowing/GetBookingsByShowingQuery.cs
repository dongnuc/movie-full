using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Bookings.Queries.GetBookingsByShowing
{
    public sealed record GetBookingsByShowingQuery(Guid ShowingId, Guid? UserTheaterId) : IQuery<List<ShowingBookingResponse>>;
}
