namespace Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat
{
    public interface ISeatLockService
    {
        Task<bool> LockSeatsAsync(Guid showingId, List<string> seatIds, string userId);
        Task<bool> UnlockSeatsAsync(Guid showingId, List<string> seatIds, string userId);
        Task<int> CountLockedSeatsAsync(Guid showingId);
        Task<List<string>> GetUserLockedSeatsAsync(Guid showingId, string userId);
        Task<(List<string> locked, List<string> unlocked, List<string> failed)>
            SyncUserSeatsAsync(Guid showingId, List<string> requestSeats, string userId);
        
        /// <summary>
        /// Lấy toàn bộ seat đang hold của tất cả user theo showing
        /// </summary>
        Task<List<Guid>> GetAllLockedSeatsByShowingAsync(Guid showingId);

        /// <summary>
        /// Lấy danh sách user đang hold ghế theo showingId và userId
        /// Returns: HashSet của seatId đang hold
        /// </summary>
        Task<HashSet<Guid>> GetUserHoldingSeatsAsync(Guid showingId, string userId);

        Task<bool> MarkSessionAsPayingAsync(Guid showingId, string userId);

        Task<int?> GetSessionRemainingSeconds(Guid showingId, string userId);
    }
}
