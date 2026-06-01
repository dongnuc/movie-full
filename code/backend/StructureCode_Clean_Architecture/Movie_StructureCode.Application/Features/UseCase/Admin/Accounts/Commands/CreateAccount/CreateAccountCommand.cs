using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.CreateAccount
{
    public sealed record CreateAccountCommand(
        string Username,
        string Email,
        string Password,
        string? RoleId,
        List<Guid>? TheaterIds
    ) : ICommand<string>;
}
