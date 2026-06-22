using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.DeleteRole
{
    public sealed class DeleteRoleCommandHandler : ICommandHander<DeleteRoleCommand>
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public DeleteRoleCommandHandler(
            RoleManager<AppRole> roleManager,
            UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<Result> Handle(
            DeleteRoleCommand command,
            CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(command.Id.ToString());
            if (role == null)
            {
                return Result.Failure(
                    new Error("Roles.RoleNotFound", $"Role with ID '{command.Id}' was not found."));
            }

            // Prevent deleting core system roles
            var coreRoles = new[] { "SYSTEMADMIN", "THEATEROWNER", "USER" };
            if (role.Name != null && coreRoles.Contains(role.Name.ToUpper()))
            {
                return Result.Failure(
                    new Error("Roles.CannotDeleteCoreRole", $"Role '{role.Name}' is a core system role and cannot be deleted."));
            }

            // Prevent deleting role if users are assigned to it
            if (!string.IsNullOrWhiteSpace(role.Name))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                if (usersInRole != null && usersInRole.Count > 0)
                {
                    return Result.Failure(
                        new Error("Roles.RoleInUse", $"Cannot delete role '{role.Name}' because it is currently assigned to {usersInRole.Count} user(s)."));
                }
            }

            var deleteResult = await _roleManager.DeleteAsync(role);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                return Result.Failure(
                    new Error("Roles.DeleteFailed", $"Failed to delete role: {errors}"));
            }

            return Result.Success();
        }
    }
}
