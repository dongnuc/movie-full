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

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.RemoveTheaterMember
{
    public sealed class RemoveTheaterMemberCommandHandler : ICommandHander<RemoveTheaterMemberCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITheaterUserRepository _theaterUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveTheaterMemberCommandHandler(
            UserManager<AppUser> userManager,
            ITheaterUserRepository theaterUserRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _theaterUserRepository = theaterUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            RemoveTheaterMemberCommand command,
            CancellationToken cancellationToken)
        {
            // Verify that member exists
            var user = await _userManager.FindByIdAsync(command.MemberId.ToString());
            if (user == null)
            {
                return Result.Failure(new Error("Members.UserNotFound", "User not found."));
            }

            // Find active membership mapping
            var mappings = (await _theaterUserRepository.FindAsync(
                tu => tu.UserId == user.Id && tu.TheaterId == command.TheaterId && tu.IsActive,
                cancellationToken)).ToList();

            var mapping = mappings.FirstOrDefault();
            if (mapping == null)
            {
                return Result.Failure(new Error("Members.MembershipNotFound", "User is not an active member of this theater."));
            }

            // Deactivate mapping
            mapping.UpdateDetails(user.Id, command.TheaterId, isPrimary: 0, isActive: false);
            _theaterUserRepository.Update(mapping);

            // Clean up theater specific roles and ensure USER role is assigned
            var currentRoles = await _userManager.GetRolesAsync(user);
            var theaterRoles = new[] { RoleConstants.BoxOfficeStaff, RoleConstants.ManagementTheater, RoleConstants.AdminTheater };
            var rolesToRemove = currentRoles.Intersect(theaterRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return Result.Failure(new Error("Members.RoleUpdateFailed", $"Failed to remove old roles: {errors}"));
                }
            }

            var updatedRoles = await _userManager.GetRolesAsync(user);
            if (!updatedRoles.Any(r => r.Equals(RoleConstants.User, StringComparison.OrdinalIgnoreCase)))
            {
                var addResult = await _userManager.AddToRoleAsync(user, RoleConstants.User);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    return Result.Failure(new Error("Members.RoleUpdateFailed", $"Failed to assign USER role: {errors}"));
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
