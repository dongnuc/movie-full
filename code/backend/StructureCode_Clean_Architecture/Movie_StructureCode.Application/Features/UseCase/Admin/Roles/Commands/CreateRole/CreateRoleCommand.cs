using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.CreateRole
{
    public sealed record CreateRoleCommand(string Name, Dictionary<string, int>? Permissions) : ICommand<string>;
}
