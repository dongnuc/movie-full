using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Application.Abstractions.Services.SignalR;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Commands.UnholdSeats
{
    internal sealed class UnholdSeatsCommandHandler : ICommandHander<UnholdSeatsCommand, bool>
    {
        private readonly ISeatLockService _seatLockService;
        private readonly ISeatHubService _seatHubService;

        public UnholdSeatsCommandHandler(ISeatLockService seatLockService, ISeatHubService seatHubService)
        {
            _seatLockService = seatLockService;
            _seatHubService = seatHubService;
        }

        public async Task<Result<bool>> Handle(UnholdSeatsCommand request, CancellationToken cancellationToken)
        {
            if (request.SeatIds == null || !request.SeatIds.Any())
            {
                return Result.Failure<bool>(new Error("SeatHolding.NoSeatsSelected", "Vui lòng chọn ít nhất một ghế để hủy giữ."));
            }

            var success = await _seatLockService.UnlockSeatsAsync(request.ShowingId, request.SeatIds, request.UserId.ToString());
            if (!success)
            {
                return Result.Failure<bool>(new Error("SeatHolding.UnlockFailed", "Không thể hủy giữ ghế."));
            }

            // Broadcast unlock notifications to other clients watching this showing
            try
            {
                var seatGuids = request.SeatIds.Select(Guid.Parse).ToList();
                await _seatHubService.NotifySeatsUnlockedAsync(request.ShowingId, seatGuids);
            }
            catch (Exception)
            {
                // SignalR broadcast error is non-blocking
            }

            return Result.Success(true);
        }
    }
}
