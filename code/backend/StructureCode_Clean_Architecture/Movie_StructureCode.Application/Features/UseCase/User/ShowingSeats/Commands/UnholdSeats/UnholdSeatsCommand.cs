using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.UnholdSeats
{
    public sealed record UnholdSeatsCommand(
        Guid ShowingId,
        List<string> SeatIds,
        Guid UserId
    ) : ICommand<bool>;
}
