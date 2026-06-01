using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.DeleteRoom
{
    public sealed record DeleteRoomCommand(Guid Id) : ICommand;
}
