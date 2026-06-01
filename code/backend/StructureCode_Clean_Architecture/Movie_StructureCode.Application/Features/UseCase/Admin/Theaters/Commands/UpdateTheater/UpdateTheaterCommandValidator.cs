using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.UpdateTheater
{
    public sealed class UpdateTheaterCommandValidator : AbstractValidator<UpdateTheaterCommand>
    {
        public UpdateTheaterCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Theater ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Theater name is required.")
                .MaximumLength(200).WithMessage("Theater name must not exceed 200 characters.");

            RuleFor(x => x.Location)
                .MaximumLength(500).WithMessage("Location must not exceed 500 characters.");
        }
    }
}
