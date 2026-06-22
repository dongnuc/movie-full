using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandHandler : ICommandHander<CreateRoleCommand, string>
    {
        private readonly RoleManager<AppRole> _roleManager;

        public CreateRoleCommandHandler(RoleManager<AppRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Result<string>> Handle(
            CreateRoleCommand command,
            CancellationToken cancellationToken)
        {
            var uppercaseName = command.Name.Trim().ToUpper();

            var roleExists = await _roleManager.RoleExistsAsync(uppercaseName);
            if (roleExists)
            {
                return Result.Failure<string>(
                    new Error("Roles.RoleAlreadyExists", $"Role '{uppercaseName}' already exists."));
            }

            var id = Guid.NewGuid();
            var newRole = new AppRole
            {
                Id = id,
                Name = uppercaseName,
                NormalizedName = uppercaseName,
                PermissionMask = 0
            };

            var createResult = await _roleManager.CreateAsync(newRole);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result.Failure<string>(
                    new Error("Roles.CreateFailed", $"Failed to create role: {errors}"));
            }

            if (command.Permissions != null)
            {
                foreach (var kvp in command.Permissions)
                {
                    var claimType = kvp.Key.Trim().ToUpper();
                    if (!PermissionConstants.AllCategories.Contains(claimType))
                    {
                        return Result.Failure<string>(
                            new Error("Roles.InvalidPermissionCategory", $"Permission category '{claimType}' is not valid."));
                    }
                    var claimValue = kvp.Value.ToString();
                    await _roleManager.AddClaimAsync(newRole, new System.Security.Claims.Claim(claimType, claimValue));
                }
            }

            return Result.Success($"Role '{newRole.Name}' created successfully.");
        }
    }
}
