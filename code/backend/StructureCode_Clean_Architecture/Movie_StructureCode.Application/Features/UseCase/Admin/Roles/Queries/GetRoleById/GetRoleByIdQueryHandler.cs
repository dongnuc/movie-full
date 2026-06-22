using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetRoleById
{
    internal sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleResponse>
    {
        private readonly RoleManager<AppRole> _roleManager;

        public GetRoleByIdQueryHandler(RoleManager<AppRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Result<RoleResponse>> Handle(
            GetRoleByIdQuery request,
            CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (role == null)
            {
                return Result.Failure<RoleResponse>(
                    new Error("Roles.RoleNotFound", $"Role with ID '{request.Id}' was not found."));
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var claim in claims)
            {
                var category = claim.Type.Trim().ToUpper();
                if (int.TryParse(claim.Value, out int mask))
                {
                    permissions[category] = mask;
                }
            }

            return Result.Success(new RoleResponse(role.Id, role.Name ?? string.Empty, permissions));
        }
    }
}
