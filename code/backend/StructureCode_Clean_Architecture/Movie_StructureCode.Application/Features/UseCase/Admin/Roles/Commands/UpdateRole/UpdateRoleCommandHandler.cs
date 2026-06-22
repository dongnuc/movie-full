using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.UpdateRole
{
    public sealed class UpdateRoleCommandHandler : ICommandHander<UpdateRoleCommand>
    {
        private readonly RoleManager<AppRole> _roleManager;

        public UpdateRoleCommandHandler(RoleManager<AppRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Result> Handle(
            UpdateRoleCommand command,
            CancellationToken cancellationToken)
        {
            var uppercaseName = command.Name.ToUpper();

            var role = await _roleManager.FindByIdAsync(command.Id.ToString());
            if (role == null)
            {
                return Result.Failure(
                    new Error("Roles.RoleNotFound", $"Role with ID '{command.Id}' was not found."));
            }

            // Check if name is taken by another role
            var roleExist = await _roleManager.FindByNameAsync(uppercaseName);
            if (roleExist != null && roleExist.Id != role.Id)
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

            var existingClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in existingClaims)
            {
                if (PermissionConstants.AllCategories.Contains(claim.Type.ToUpper()) ||
                    int.TryParse(claim.Value, out _))
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
            }

            if (command.Permissions != null)
            {
                foreach (var kvp in command.Permissions)
                {
                    var claimType = kvp.Key.Trim().ToUpper();
                    if (!PermissionConstants.AllCategories.Contains(claimType))
                    {
                        return Result.Failure(
                            new Error("Roles.InvalidPermissionCategory", $"Permission category '{claimType}' is not valid."));
                    }
                    var claimValue = kvp.Value.ToString();
                    await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(claimType, claimValue));
                }
            }

            return Result.Success();
        }
    }
}
