using FluentValidation;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Commands.UpdateShowing
{
    public sealed class UpdateShowingCommandValidator : AbstractValidator<UpdateShowingCommand>
    {
        public UpdateShowingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID lịch chiếu không được để trống.");

            RuleFor(x => x.TimeStart)
                .NotEmpty().WithMessage("Thời gian bắt đầu không được để trống.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Giá vé không được nhỏ hơn 0.");

            RuleFor(x => x.MaxTicketsPerUser)
                .GreaterThan(0).WithMessage("Số lượng vé tối đa cho mỗi người dùng phải lớn hơn 0.");

            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("ID phòng không được để trống.");
        }
    }
}
