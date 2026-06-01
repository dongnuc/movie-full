using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Accounts.Commands.ToggleAccountStatus
{
    public sealed class ToggleAccountStatusCommandHandler : ICommandHander<ToggleAccountStatusCommand>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITheaterUserRepository _theaterUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleAccountStatusCommandHandler(
            UserManager<AppUser> userManager,
            ITheaterUserRepository theaterUserRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _theaterUserRepository = theaterUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ToggleAccountStatusCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.Id.ToString());
            if (user == null)
            {
                return Result.Failure(
                    new Error("Accounts.UserNotFound", $"User with ID '{command.Id}' was not found."));
            }

            var isCurrentlyLocked = await _userManager.IsLockedOutAsync(user);

            if (isCurrentlyLocked)
            {
                // Unlock account
                user.LockoutEnd = null;
            }
            else
            {
                // Lock account for 10 years (Deactivate)
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(10);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure(
                    new Error("Accounts.ToggleStatusFailed", $"Failed to update lockout status: {errors}"));
            }

            // If we deactivated the user, deactivate all their theater assignments as well
            if (!isCurrentlyLocked)
            {
                var activeMappings = await _theaterUserRepository.FindAsync(tu => tu.UserId == user.Id && tu.IsActive, cancellationToken);
                foreach (var mapping in activeMappings)
                {
                    mapping.UpdateDetails(user.Id, mapping.TheaterId, isPrimary: 0, isActive: false);
                    _theaterUserRepository.Update(mapping);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Success();
        }
    }
}
