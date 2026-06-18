using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.MarkSessionAsPaying
{
    public sealed record MarkSessionAsPayingCommand(
        Guid ShowingId,
        Guid UserId
    ) : ICommand<int?>;
}
