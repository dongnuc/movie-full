using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Application.Abstractions.Services.SignalR;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.HoldSeats
{
    internal sealed class HoldSeatsCommandHandler : ICommandHander<HoldSeatsCommand, bool>
    {
        private readonly ISeatLockService _seatLockService;
        private readonly ISeatHubService _seatHubService;

        public HoldSeatsCommandHandler(ISeatLockService seatLockService, ISeatHubService seatHubService)
        {
            _seatLockService = seatLockService;
            _seatHubService = seatHubService;
        }

        public async Task<Result<bool>> Handle(HoldSeatsCommand request, CancellationToken cancellationToken)
        {
            if (request.SeatIds == null || !request.SeatIds.Any())
            {
                return Result.Failure<bool>(new Error("SeatHolding.NoSeatsSelected", "Vui lòng chọn ít nhất một ghế để giữ."));
            }

            var success = await _seatLockService.LockSeatsAsync(request.ShowingId, request.SeatIds, request.UserId.ToString());
            if (!success)
            {
                return Result.Failure<bool>(new Error("SeatHolding.LockFailed", "Không thể giữ ghế. Ghế có thể đã bị chọn hoặc đã hết hạn giữ."));
            }

            // Broadcast lock notifications to other clients watching this showing
            try
            {
                var seatGuids = request.SeatIds.Select(Guid.Parse).ToList();
                await _seatHubService.NotifySeatsLockedAsync(request.ShowingId, seatGuids, request.UserId.ToString());
            }
            catch (Exception)
            {
                // SignalR broadcast error is non-blocking
            }

            return Result.Success(true);
        }
    }
}
