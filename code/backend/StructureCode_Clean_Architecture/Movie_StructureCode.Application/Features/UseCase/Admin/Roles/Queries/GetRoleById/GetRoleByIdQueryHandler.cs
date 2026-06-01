using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Queries.GetRoleById
{
    internal sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleResponse>
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public GetRoleByIdQueryHandler(RoleManager<IdentityRole<Guid>> roleManager)
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

            return Result.Success(new RoleResponse(role.Id, role.Name ?? string.Empty));
        }
    }
}
