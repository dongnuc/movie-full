using Movie_StructureCode.Contract.Abstractions.Message;
using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.UpdateRole
{
    public sealed record UpdateRoleCommand(Guid Id, string Name) : ICommand;
}
