using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsToCouple
{
    public sealed record UpdateSeatsToCoupleCommand(List<Guid> SeatIds) : ICommand;
}
