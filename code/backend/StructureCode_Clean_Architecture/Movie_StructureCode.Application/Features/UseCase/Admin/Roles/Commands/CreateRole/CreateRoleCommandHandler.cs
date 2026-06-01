using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandHandler : ICommandHander<CreateRoleCommand, string>
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public CreateRoleCommandHandler(RoleManager<IdentityRole<Guid>> roleManager)
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
            var newRole = new IdentityRole<Guid>
            {
                Id = id,
                Name = uppercaseName,
                NormalizedName = uppercaseName
            };

            var createResult = await _roleManager.CreateAsync(newRole);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result.Failure<string>(
                    new Error("Roles.CreateFailed", $"Failed to create role: {errors}"));
            }

            return Result.Success($"Role '{newRole.Name}' created successfully.");
        }
    }
}
