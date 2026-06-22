using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.UpdateAccount
{
    public sealed record UpdateAccountCommand(
        Guid Id,
        string Username,
        string Email,
        List<Guid> RoleIds,
        List<Guid>? TheaterIds
    ) : ICommand;
}
