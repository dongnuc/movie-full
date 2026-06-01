using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsTypeInBulk
{
    public sealed class UpdateSeatsTypeInBulkCommandValidator : AbstractValidator<UpdateSeatsTypeInBulkCommand>
    {
        public UpdateSeatsTypeInBulkCommandValidator()
        {
            RuleFor(x => x.SeatIds)
                .NotEmpty().WithMessage("Seat IDs are required.");

            RuleFor(x => x.SeatTypeId)
                .NotEmpty().WithMessage("SeatType ID is required.");
        }
    }
}
