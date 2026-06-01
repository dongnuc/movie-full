using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsToCouple
{
    public sealed class UpdateSeatsToCoupleCommandValidator : AbstractValidator<UpdateSeatsToCoupleCommand>
    {
        public UpdateSeatsToCoupleCommandValidator()
        {
            RuleFor(x => x.SeatIds)
                .NotEmpty().WithMessage("Danh sách ghế không được để trống.")
                .Must(ids => ids != null && ids.Count % 2 == 0)
                .WithMessage("Số lượng ghế chọn phải là số chẵn.");
        }
    }
}
