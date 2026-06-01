using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Commands.UpdateShowing
{
    public sealed record UpdateShowingCommand(
        Guid Id,
        DateTime TimeStart,
        decimal Price,
        int MaxTicketsPerUser,
        Guid RoomId,
        bool IsActive
    ) : ICommand;
}
