using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.UpdateRole
{
    public sealed class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Role ID is required.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Role Name is required.");
        }
    }
}
