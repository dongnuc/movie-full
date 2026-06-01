using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.CreateSeatType
{
    public sealed record CreateSeatTypeCommand(
        string Name,
        decimal Surcharge,
        bool IsActive = true
    ) : ICommand;
}
