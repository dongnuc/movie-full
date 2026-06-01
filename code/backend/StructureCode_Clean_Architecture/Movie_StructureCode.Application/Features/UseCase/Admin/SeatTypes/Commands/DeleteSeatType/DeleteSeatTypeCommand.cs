using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.DeleteSeatType
{
    public sealed record DeleteSeatTypeCommand(Guid Id) : ICommand;
}
