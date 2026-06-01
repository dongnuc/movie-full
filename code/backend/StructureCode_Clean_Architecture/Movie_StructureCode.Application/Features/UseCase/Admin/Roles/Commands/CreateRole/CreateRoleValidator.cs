using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Roles.Commands.CreateRole
{
    public sealed class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Role Name is required.");
        }
    }
}
