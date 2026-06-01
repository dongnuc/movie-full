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

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.UpdateAccount
{
    public sealed class UpdateAccountCommandHandler : ICommandHander<UpdateAccountCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITheaterUserRepository _theaterUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAccountCommandHandler(
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

        public async Task<Result> Handle(
            UpdateAccountCommand command,
            CancellationToken cancellationToken)
        {
            // Retrieve role from Identity store using RoleId
            var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                return Result.Failure(
                    new Error("Accounts.RoleNotFound", $"Role with ID '{command.RoleId}' was not found."));
            }

            // Find user
            var user = await _userManager.FindByIdAsync(command.Id.ToString());
            if (user == null)
            {
                return Result.Failure(
                    new Error("Accounts.UserNotFound", $"User with ID '{command.Id}' was not found."));
            }

            // Verify username uniqueness
            var userWithUsername = await _userManager.FindByNameAsync(command.Username);
            if (userWithUsername != null && userWithUsername.Id != user.Id)
            {
                return Result.Failure(
                    new Error("Accounts.UsernameTaken", $"Username '{command.Username}' is already taken."));
            }

            // Verify email uniqueness
            var userWithEmail = await _userManager.FindByEmailAsync(command.Email);
            if (userWithEmail != null && userWithEmail.Id != user.Id)
            {
                return Result.Failure(
                    new Error("Accounts.EmailTaken", $"Email '{command.Email}' is already registered."));
            }

            // Update user properties
            user.UserName = command.Username;
            user.Email = command.Email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result.Failure(
                    new Error("Accounts.UpdateFailed", $"Failed to update user: {errors}"));
            }

            // Update Role assignment
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(role.Name))
            {
                // Remove from all current roles
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                // Add to new role
                var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Result.Failure(
                        new Error("Accounts.RoleAssignFailed", $"Failed to assign role: {errors}"));
                }
            }

            // Update Theater Associations
            var existingMappings = await _theaterUserRepository.FindAsync(tu => tu.UserId == user.Id, cancellationToken);
            var targetTheaterIds = command.TheaterIds ?? new List<Guid>();

            if (role.Name == "TheaterOwner")
            {
                // Sync mappings:
                // 1. Process existing mappings
                bool hasPrimary = false;
                foreach (var mapping in existingMappings)
                {
                    if (targetTheaterIds.Contains(mapping.TheaterId))
                    {
                        // Keep mapping, activate if inactive
                        int isPrimary = !hasPrimary ? 1 : 0;
                        hasPrimary = true;
                        mapping.UpdateDetails(user.Id, mapping.TheaterId, isPrimary, isActive: true);
                        _theaterUserRepository.Update(mapping);
                        targetTheaterIds.Remove(mapping.TheaterId); // Already handled
                    }
                    else
                    {
                        // Remove association by deactivating (soft delete)
                        mapping.UpdateDetails(user.Id, mapping.TheaterId, isPrimary: 0, isActive: false);
                        _theaterUserRepository.Update(mapping);
                    }
                }

                // 2. Add remaining targetTheaterIds (new mappings)
                foreach (var theaterId in targetTheaterIds)
                {
                    int isPrimary = !hasPrimary ? 1 : 0;
                    hasPrimary = true;
                    var mapping = Theater_User.Create(
                        Guid.NewGuid(),
                        user.Id,
                        theaterId,
                        isPrimary
                    );
                    await _theaterUserRepository.AddAsync(mapping, cancellationToken);
                }
            }
            else
            {
                // Deactivate all existing mappings if no longer a TheaterOwner
                foreach (var mapping in existingMappings)
                {
                    if (mapping.IsActive)
                    {
                        mapping.UpdateDetails(user.Id, mapping.TheaterId, isPrimary: 0, isActive: false);
                        _theaterUserRepository.Update(mapping);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
