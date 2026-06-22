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
using StackExchange.Redis;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.AddTheaterMember
{
    public sealed class AddTheaterMemberCommandHandler : ICommandHander<AddTheaterMemberCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITheaterUserRepository _theaterUserRepository;
        private readonly ITheaterRepository _theaterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddTheaterMemberCommandHandler(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ITheaterUserRepository theaterUserRepository,
            ITheaterRepository theaterRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _theaterUserRepository = theaterUserRepository;
            _theaterRepository = theaterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            AddTheaterMemberCommand command,
            CancellationToken cancellationToken)
        {
            // Verify theater exists
            var theaterExists = await _theaterRepository.AnyAsync(t => t.Id == command.TheaterId, cancellationToken);
            if (!theaterExists)
            {
                return Result.Failure(new Error("Theater.NotFound", "Theater not found."));
            }

            // Find user by username
            var user = await _userManager.FindByNameAsync(command.Username);
            if (user == null)
            {
                return Result.Failure(new Error("Members.UserNotFound", $"User with username '{command.Username}' was not found."));
            }

            // Verify role
            var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                return Result.Failure(new Error("Members.RoleNotFound", "Selected role not found."));
            }
            // Validate that it's a valid theater staff/member role
            var validRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                RoleConstants.BoxOfficeStaff,
                RoleConstants.ManagementTheater,
                RoleConstants.AdminTheater
            };

            if (!validRoles.Contains(role.Name))
            {
                return Result.Failure(new Error("Members.InvalidRole", "Selected role is not a valid theater role."));
            }

            // Check existing theater mapping
            var existingMappings = (await _theaterUserRepository.FindAsync(
                tu => tu.UserId == user.Id && tu.TheaterId == command.TheaterId, cancellationToken)).ToList();

            var mapping = existingMappings.FirstOrDefault();
            if (mapping != null)
            {
                if (mapping.IsActive == true)
                {
                    return Result.Failure(new Error("Members.AlreadyExists", $"User '{command.Username}' is already a member of this theater."));
                }
                else
                {
                    // Reactivate
                    mapping.UpdateDetails(user.Id, command.TheaterId, isPrimary: 1, isActive: true);
                    _theaterUserRepository.Update(mapping);
                }
            }
            else
            {
                // Create new mapping
                var newMapping = Theater_User.Create(
                    Guid.NewGuid(),
                    user.Id,
                    command.TheaterId,
                    isPrimary: 1
                );
                await _theaterUserRepository.AddAsync(newMapping, cancellationToken);
            }

            // Update user roles: remove current theater/consumer roles, and add the new one
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Intersect(
                new[] { RoleConstants.BoxOfficeStaff, RoleConstants.ManagementTheater, RoleConstants.AdminTheater, RoleConstants.User },
                StringComparer.OrdinalIgnoreCase
            ).ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return Result.Failure(new Error("Members.RoleUpdateFailed", $"Failed to remove old roles: {errors}"));
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return Result.Failure(new Error("Members.RoleUpdateFailed", $"Failed to assign new role: {errors}"));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
