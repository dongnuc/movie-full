using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.ToggleAccountStatus
{
    public sealed record ToggleAccountStatusCommand(Guid Id) : ICommand;
}
