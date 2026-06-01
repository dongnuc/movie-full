using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Commands.CreateShowing
{
    public sealed record CreateShowingCommand(
        DateTime TimeStart,
        decimal Price,
        int MaxTicketsPerUser,
        Guid MovieId,
        Guid RoomId,
        bool IsActive = true
    ) : ICommand;
}
