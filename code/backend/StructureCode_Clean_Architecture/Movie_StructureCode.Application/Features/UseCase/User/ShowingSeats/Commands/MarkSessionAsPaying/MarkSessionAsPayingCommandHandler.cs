using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.MarkSessionAsPaying
{
    internal sealed class MarkSessionAsPayingCommandHandler : ICommandHander<MarkSessionAsPayingCommand, int?>
    {
        private readonly ISeatLockService _seatLockService;

        public MarkSessionAsPayingCommandHandler(ISeatLockService seatLockService)
        {
            _seatLockService = seatLockService;
        }

        public async Task<Result<int?>> Handle(MarkSessionAsPayingCommand request, CancellationToken cancellationToken)
        {
            var remainingSeconds = await _seatLockService.MarkSessionAsPayingAsync(request.ShowingId, request.UserId.ToString());
            if (remainingSeconds == null)
            {
                return Result.Failure<int?>(new Error("SeatPaying.ExtendFailed", "Không thể gia hạn session hoặc session không tồn tại."));
            }

            return Result.Success(remainingSeconds);
        }
    }
}
