using Movie_StructureCode.Contract.Abstractions.Command;

namespace Movie_StructureCode.Application.Abstractions.Services.SignalR
{
    /// <summary>
    /// Abstraction for real-time seat notification via SignalR.
    /// Application layer depends only on this interface – not on any SignalR concrete types.
    /// </summary>
    public interface ISeatHubService
    {
        /// <summary>
        /// Broadcast to all clients watching a Showing that a batch of seats
        /// has just been locked (selected) by a user.
        /// </summary>
        /// <param name="showingId">ID of the showing.</param>
        /// <param name="seatIds">List of seat IDs that were locked.</param>
        /// <param name="lockedByUserId">ID of the user who locked the seats.</param>
        Task NotifySeatsLockedAsync(Guid showingId, List<Guid> seatIds, string lockedByUserId);

        /// <summary>
        /// Broadcast to all clients watching a Showing that a batch of seats
        /// has just been unlocked (deselected).
        /// </summary>
        /// <param name="showingId">ID of the showing.</param>
        /// <param name="seatIds">List of seat IDs that were unlocked.</param>
        Task NotifySeatsUnlockedAsync(Guid showingId, List<Guid> seatIds);

        /// <summary>
        /// Notify a specific user that their seat selection session is about to timeout.
        /// </summary>
        /// <param name="showingId">ID of the showing.</param>
        /// <param name="userId">ID of the user.</param>
        /// <param name="remainingSeconds">Seconds remaining until expiration.</param>
        Task NotifySessionTimeoutWarningAsync(Guid showingId, string userId, int remainingSeconds);

        /// <summary>
        /// Broadcast to all clients watching a Showing that a batch of seats
        /// has just been booked.
        /// </summary>
        Task NotifySeatsBookedAsync(Guid showingId, List<Guid> seatIds);
    }
}
