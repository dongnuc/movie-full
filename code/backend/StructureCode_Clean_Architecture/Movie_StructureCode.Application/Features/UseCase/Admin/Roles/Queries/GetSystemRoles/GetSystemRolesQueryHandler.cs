using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetSystemRoles
{
    internal sealed class GetSystemRolesQueryHandler : IQueryHandler<GetSystemRolesQuery, List<RoleResponse>>
    {
        private readonly RoleManager<AppRole> _roleManager;

        public GetSystemRolesQueryHandler(RoleManager<AppRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Result<List<RoleResponse>>> Handle(
            GetSystemRolesQuery request,
            CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
            var response = new List<RoleResponse>();

            foreach (var r in roles)
            {
                if (r.Name == null || !r.Name.Contains("SYSTEM", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

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
                response.Add(new RoleResponse(r.Id, r.Name, permissions));
            }

            return Result.Success(response);
        }
    }
}
