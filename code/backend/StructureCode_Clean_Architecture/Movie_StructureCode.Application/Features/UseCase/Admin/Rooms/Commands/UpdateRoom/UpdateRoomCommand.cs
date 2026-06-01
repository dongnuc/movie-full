using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.UpdateRoom
{
    public sealed record UpdateRoomCommand(
        Guid Id,
        string Name,
        int TotalRow,
        int TotalCol,
        Guid TheaterId,
        bool IsActive
    ) : ICommand;
}
