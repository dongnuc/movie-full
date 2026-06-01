using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.UpdateRole
{
    public sealed class UpdateRoleCommandHandler : ICommandHander<UpdateRoleCommand>
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public UpdateRoleCommandHandler(RoleManager<IdentityRole<Guid>> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Result> Handle(
            UpdateRoleCommand command,
            CancellationToken cancellationToken)
        {
            var uppercaseName = command.Name.Trim().ToUpper();

            var role = await _roleManager.FindByIdAsync(command.Id.ToString());
            if (role == null)
            {
                return Result.Failure(
                    new Error("Roles.RoleNotFound", $"Role with ID '{command.Id}' was not found."));
            }

            // Check if name is taken by another role
            var otherRole = await _roleManager.FindByNameAsync(uppercaseName);
            if (otherRole != null && otherRole.Id != role.Id)
            {
                return Result.Failure(
                    new Error("Roles.RoleNameTaken", $"Role name '{uppercaseName}' is already taken by another role."));
            }

            role.Name = uppercaseName;
            role.NormalizedName = uppercaseName;

            var updateResult = await _roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result.Failure(
                    new Error("Roles.UpdateFailed", $"Failed to update role: {errors}"));
            }

            return Result.Success();
        }
    }
}
