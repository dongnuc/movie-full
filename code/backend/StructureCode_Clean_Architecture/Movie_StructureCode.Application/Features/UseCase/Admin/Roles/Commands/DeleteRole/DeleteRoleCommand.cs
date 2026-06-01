using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.DeleteRole
{
    public sealed record DeleteRoleCommand(Guid Id) : ICommand;
}
