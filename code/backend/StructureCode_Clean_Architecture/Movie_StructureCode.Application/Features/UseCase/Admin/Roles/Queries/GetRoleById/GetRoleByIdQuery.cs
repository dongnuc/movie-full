using System;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetRoleById
{
    public sealed record GetRoleByIdQuery(Guid Id) : IQuery<RoleResponse>;
}
