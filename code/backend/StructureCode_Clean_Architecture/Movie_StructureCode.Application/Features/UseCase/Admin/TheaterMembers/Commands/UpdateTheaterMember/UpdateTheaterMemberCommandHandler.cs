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

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.UpdateTheaterMember
{
    public sealed class UpdateTheaterMemberCommandHandler : ICommandHander<UpdateTheaterMemberCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITheaterUserRepository _theaterUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTheaterMemberCommandHandler(
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
            UpdateTheaterMemberCommand command,
            CancellationToken cancellationToken)
        {
            // Verify that member exists
            var user = await _userManager.FindByIdAsync(command.MemberId.ToString());
            if (user == null)
            {
                return Result.Failure(new Error("Members.UserNotFound", "User not found."));
            }

            // Verify mapping exists and is active for this theater
            var mappings = (await _theaterUserRepository.FindAsync(
                tu => tu.UserId == user.Id && tu.TheaterId == command.TheaterId && tu.IsActive,
                cancellationToken)).ToList();

            if (!mappings.Any())
            {
                return Result.Failure(new Error("Members.MembershipNotFound", "User is not a member of this theater."));
            }

            // Verify role
            var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                return Result.Failure(new Error("Members.RoleNotFound", "Selected role not found."));
            }

            // Validate theater role
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

            // Sync user roles
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
