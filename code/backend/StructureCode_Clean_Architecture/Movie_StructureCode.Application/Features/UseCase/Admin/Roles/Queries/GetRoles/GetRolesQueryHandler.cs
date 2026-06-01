using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetRoles
{
    internal sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, List<RoleResponse>>
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public GetRolesQueryHandler(RoleManager<IdentityRole<Guid>> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Result<List<RoleResponse>>> Handle(
            GetRolesQuery request,
            CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleResponse(r.Id, r.Name ?? string.Empty))
                .ToListAsync(cancellationToken);

            return Result.Success(roles);
        }
    }
}
