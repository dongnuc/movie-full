using Microsoft.AspNetCore.SignalR;
using Movie_StructureCode.API.Hubs;
using Movie_StructureCode.Application.Abstractions.Services.SignalR;
using Movie_StructureCode.Contract.Abstractions.Command;

namespace Movie_StructureCode.API.Services
{
    public sealed class SeatHubService : ISeatHubService
    {
        private readonly IHubContext<SeatHub> _hubContext;

        public SeatHubService(IHubContext<SeatHub> hubContext)
        {
            _hubContext = hubContext
                ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task NotifySeatsLockedAsync(
            Guid showingId,
            List<Guid> seatIds,
            string lockedByUserId)
        {
            var groupName = SeatHub.GetGroupName(showingId.ToString());

            var payload = new SeatsLockedEvent
            {
                ShowingId = showingId,
                SeatIds = seatIds,
                LockedBy = lockedByUserId
            };

            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("SeatsLocked", payload);
        }

        public async Task NotifySeatsUnlockedAsync(
            Guid showingId,
            List<Guid> seatIds)
        {
            var groupName = SeatHub.GetGroupName(showingId.ToString());

            var payload = new SeatsUnlockedEvent
            {
                ShowingId = showingId,
                SeatIds = seatIds
            };

            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("SeatsUnlocked", payload);
        }

        public async Task NotifySessionTimeoutWarningAsync(
            Guid showingId,
            string userId,
            int remainingSeconds)
        {
            var payload = new
            {
                ShowingId = showingId,
                UserId = userId,
                RemainingSeconds = remainingSeconds,
                Message = "Sắp hết thời gian chọn ghế, hãy thanh toán nếu không số ghế đã chọn sẽ bị mất"
            };

            await _hubContext.Clients
                .User(userId)
                .SendAsync("SessionTimeoutWarning", payload);
        }
    }
}