using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.UpdateRoom
{
    public sealed class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
    {
        public UpdateRoomCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Room ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Room name is required.")
                .MaximumLength(100).WithMessage("Room name must not exceed 100 characters.");

            RuleFor(x => x.TotalRow)
                .GreaterThan(0).WithMessage("Total rows must be greater than 0.");

            RuleFor(x => x.TotalCol)
                .GreaterThan(0).WithMessage("Total columns must be greater than 0.");

            RuleFor(x => x.TheaterId)
                .NotEmpty().WithMessage("Theater ID is required.");
        }
    }
}
