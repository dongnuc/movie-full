using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.CreateSeatType
{
    public sealed class CreateSeatTypeCommandValidator : AbstractValidator<CreateSeatTypeCommand>
    {
        public CreateSeatTypeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("SeatType name is required.")
                .MaximumLength(100).WithMessage("SeatType name must not exceed 100 characters.");

            RuleFor(x => x.Surcharge)
                .GreaterThanOrEqualTo(0).WithMessage("Surcharge must be greater than or equal to 0.");
        }
    }
}
