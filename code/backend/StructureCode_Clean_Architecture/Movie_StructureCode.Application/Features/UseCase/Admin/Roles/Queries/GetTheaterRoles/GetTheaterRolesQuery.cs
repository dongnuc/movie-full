using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetTheaterRoles
{
    public sealed record GetTheaterRolesQuery : IQuery<List<RoleResponse>>;
}
