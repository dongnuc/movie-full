using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.CreateAccount
{
    public sealed class CreateAccountCommandHandler : ICommandHander<CreateAccountCommand, string>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITheaterUserRepository _theaterUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateAccountCommandHandler(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ITheaterUserRepository theaterUserRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _theaterUserRepository = theaterUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(
            CreateAccountCommand command,
            CancellationToken cancellationToken)
        {
            // Retrieve role from Identity store using RoleId
            string roleName = string.Empty;
            if(command.RoleId != null){
                var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
                if (role == null || string.IsNullOrWhiteSpace(role.Name))
                {
                    return Result.Failure<string>(
                        new Error("Accounts.RoleNotFound", $"Role with ID '{command.RoleId}' was not found."));
                }
                roleName = role.Name;
            }
            else
            {
                var role = await _roleManager.FindByNameAsync("USER");
                if (role == null || string.IsNullOrWhiteSpace(role.Name))
                {
                    return Result.Failure<string>(
                        new Error("Accounts.RoleNotFound", "Role 'USER' was not found."));
                }
                roleName = role.Name;
            }

            // Check if username already exists
            var userExists = await _userManager.FindByNameAsync(command.Username);
            if (userExists != null)
                return Result.Failure<string>(
                    new Error("Accounts.UsernameTaken", $"Username '{command.Username}' is already taken."));

            // Check if email already exists
            var emailExists = await _userManager.FindByEmailAsync(command.Email);
            if (emailExists != null)
                return Result.Failure<string>(
                    new Error("Accounts.EmailTaken", $"Email '{command.Email}' is already registered."));

            // Create new user
            var appUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = command.Username,
                Email = command.Email,
                EmailConfirmed = true, // pre-confirm admin created accounts
                LockoutEnabled = true // enable lockout so admin can lockout/deactivate accounts
            };

            var createResult = await _userManager.CreateAsync(appUser, command.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result.Failure<string>(
                    new Error("Accounts.CreateFailed", $"Failed to create account: {errors}"));
            }

            // Assign role
            var roleResult = await _userManager.AddToRoleAsync(appUser, roleName);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result.Failure<string>(
                    new Error("Accounts.RoleAssignFailed", $"Failed to assign role: {errors}"));
            }

            // Map associated theaters if TheaterOwner
            if (roleName.Equals("THEATEROWNER", StringComparison.OrdinalIgnoreCase) && command.TheaterIds != null && command.TheaterIds.Count > 0)
            {
                for (int i = 0; i < command.TheaterIds.Count; i++)
                {
                    var mapping = Theater_User.Create(
                        Guid.NewGuid(),
                        appUser.Id,
                        command.TheaterIds[i],
                        isPrimary: i == 0 ? 1 : 0
                    );
                    await _theaterUserRepository.AddAsync(mapping, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Success($"Account '{appUser.UserName}' has been created successfully.");
        }
    }
}
