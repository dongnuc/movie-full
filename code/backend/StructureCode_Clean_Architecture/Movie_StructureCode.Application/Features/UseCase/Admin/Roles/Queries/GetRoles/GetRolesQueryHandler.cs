using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetRoles
{
    internal sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, List<RoleResponse>>
    {
        private readonly RoleManager<AppRole> _roleManager;

        public GetRolesQueryHandler(RoleManager<AppRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Result<List<RoleResponse>>> Handle(
            GetRolesQuery request,
            CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
            var response = new List<RoleResponse>();

            foreach (var r in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(r);
                var permissions = new Dictionary<string, int>();
                foreach (var claim in claims)
                {
                    var category = claim.Type.Trim().ToUpper();
                    if (int.TryParse(claim.Value, out int mask))
                    {
                        permissions[category] = mask;
                    }
                }
                response.Add(new RoleResponse(r.Id, r.Name ?? string.Empty, permissions));
            }

            return Result.Success(response);
        }
    }
}
