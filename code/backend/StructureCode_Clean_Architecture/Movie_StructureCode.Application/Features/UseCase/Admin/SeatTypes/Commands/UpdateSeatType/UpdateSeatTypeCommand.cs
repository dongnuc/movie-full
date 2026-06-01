using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.UpdateSeatType
{
    public sealed record UpdateSeatTypeCommand(
        Guid Id,
        string Name,
        decimal Surcharge,
        bool IsActive
    ) : ICommand;
}
