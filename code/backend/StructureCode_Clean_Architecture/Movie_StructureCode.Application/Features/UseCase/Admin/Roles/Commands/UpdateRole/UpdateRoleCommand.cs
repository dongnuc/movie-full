using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.UpdateRole
{
    public sealed record UpdateRoleCommand(Guid Id, string Name, Dictionary<string, int>? Permissions) : ICommand;
}
