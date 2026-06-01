using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.CreateRoom
{
    public sealed record CreateRoomCommand(
        string Name,
        int TotalRow,
        int TotalCol,
        Guid TheaterId,
        bool IsActive = true
    ) : ICommand;
}
