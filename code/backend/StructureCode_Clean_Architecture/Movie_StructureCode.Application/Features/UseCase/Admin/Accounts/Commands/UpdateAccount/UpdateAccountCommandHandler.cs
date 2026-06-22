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
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITheaterUserRepository _theaterUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAccountCommandHandler(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
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
            // ── 1. Validate & resolve roles ───────────────────────────────────
            var resolvedRoles = new List<AppRole>();
            foreach (var roleId in command.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null || string.IsNullOrWhiteSpace(role.Name))
                    return Result.Failure(
                        new Error("Accounts.RoleNotFound",
                                  $"Role with ID '{roleId}' was not found."));
                resolvedRoles.Add(role);
            }

            // ── 2. Find user ──────────────────────────────────────────────────
            var user = await _userManager.FindByIdAsync(command.Id.ToString());
            if (user == null)
                return Result.Failure(
                    new Error("Accounts.UserNotFound",
                              $"User with ID '{command.Id}' was not found."));

            // ── 3. Username uniqueness check ──────────────────────────────────
            var userWithUsername = await _userManager.FindByNameAsync(command.Username);
            if (userWithUsername != null && userWithUsername.Id != user.Id)
                return Result.Failure(
                    new Error("Accounts.UsernameTaken",
                              $"Username '{command.Username}' is already taken."));

            // ── 4. Email uniqueness check ─────────────────────────────────────
            var userWithEmail = await _userManager.FindByEmailAsync(command.Email);
            if (userWithEmail != null && userWithEmail.Id != user.Id)
                return Result.Failure(
                    new Error("Accounts.EmailTaken",
                              $"Email '{command.Email}' is already registered."));

            // ── 5. Update basic user properties ──────────────────────────────
            user.UserName = command.Username;
            user.Email    = command.Email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result.Failure(
                    new Error("Accounts.UpdateFailed", $"Failed to update user: {errors}"));
            }

            // ── 6. Sync roles (set-based diff) ────────────────────────────────
            var desiredRoleNames = resolvedRoles
                .Select(r => r.Name!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var currentRoles = (await _userManager.GetRolesAsync(user))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Remove stale roles
            var rolesToRemove = currentRoles
                .Where(r => !desiredRoleNames.Contains(r))
                .ToList();
            if (rolesToRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return Result.Failure(
                        new Error("Accounts.RoleRemoveFailed", $"Failed to remove roles: {errors}"));
                }
            }

            // Add new roles
            var rolesToAdd = desiredRoleNames
                .Where(r => !currentRoles.Contains(r))
                .ToList();
            if (rolesToAdd.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    return Result.Failure(
                        new Error("Accounts.RoleAssignFailed", $"Failed to assign roles: {errors}"));
                }
            }

            // ── 7. Sync theater associations ──────────────────────────────────
            bool isTheaterAdmin = desiredRoleNames.Any(r =>
                r.Equals("ADMIN-THEATER", StringComparison.OrdinalIgnoreCase));

            var existingMappings = (await _theaterUserRepository
                .FindAsync(tu => tu.UserId == user.Id, cancellationToken))
                .ToList();

            if (isTheaterAdmin)
            {
                // Build a mutable set of theater IDs still to be handled
                var pendingTheaterIds = (command.TheaterIds ?? new List<Guid>())
                    .ToHashSet();

                bool hasPrimary = false;

                // Process existing mappings first
                foreach (var mapping in existingMappings)
                {
                    if (pendingTheaterIds.Contains(mapping.TheaterId))
                    {
                        // Keep / reactivate
                        int isPrimary = hasPrimary ? 0 : 1;
                        hasPrimary = true;
                        mapping.UpdateDetails(user.Id, mapping.TheaterId, isPrimary, isActive: true);
                        _theaterUserRepository.Update(mapping);
                        pendingTheaterIds.Remove(mapping.TheaterId); // mark as handled
                    }
                    else
                    {
                        // Soft-delete: no longer in the desired set
                        mapping.UpdateDetails(user.Id, mapping.TheaterId, isPrimary: 0, isActive: false);
                        _theaterUserRepository.Update(mapping);
                    }
                }

                // Add brand-new theater mappings
                foreach (var theaterId in pendingTheaterIds)
                {
                    int isPrimary = hasPrimary ? 0 : 1;
                    hasPrimary = true;
                    var newMapping = Theater_User.Create(
                        Guid.NewGuid(),
                        user.Id,
                        theaterId,
                        isPrimary);
                    await _theaterUserRepository.AddAsync(newMapping, cancellationToken);
                }
            }
            else
            {
                // Not a theater admin anymore → deactivate all existing mappings
                foreach (var mapping in existingMappings.Where(m => m.IsActive))
                {
                    mapping.UpdateDetails(user.Id, mapping.TheaterId, isPrimary: 0, isActive: false);
                    _theaterUserRepository.Update(mapping);
                }
            }

            // ── 8. Commit ─────────────────────────────────────────────────────
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
