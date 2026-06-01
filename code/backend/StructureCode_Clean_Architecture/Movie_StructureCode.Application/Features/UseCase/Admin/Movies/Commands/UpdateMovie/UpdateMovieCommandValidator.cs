using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.UpdateMovie
{
    public sealed class UpdateMovieCommandValidator : AbstractValidator<UpdateMovieCommand>
    {
        public UpdateMovieCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Movie ID is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Movie title is required.")
                .MaximumLength(250).WithMessage("Movie title must not exceed 250 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Base price must be greater than or equal to 0.");

            RuleFor(x => x.Duration)
                .GreaterThan(0).WithMessage("Duration must be greater than 0.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.");
        }
    }
}
