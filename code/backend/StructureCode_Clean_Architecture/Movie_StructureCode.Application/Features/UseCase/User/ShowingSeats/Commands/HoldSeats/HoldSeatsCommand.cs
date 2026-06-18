using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.HoldSeats
{
    public sealed record HoldSeatsResponse(Guid? SessionId, int? RemainingSeconds);

    public sealed record HoldSeatsCommand(
        Guid ShowingId,
        List<string> SeatIds,
        Guid UserId
    ) : ICommand<HoldSeatsResponse>;
}
