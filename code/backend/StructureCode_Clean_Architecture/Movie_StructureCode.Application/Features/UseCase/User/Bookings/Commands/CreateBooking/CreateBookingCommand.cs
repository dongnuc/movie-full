using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Bookings.Commands.CreateBooking
{
    public sealed record CreateBookingCommand(
        Guid ShowingId,
        Guid UserId
    ) : ICommand<Guid>;
}
