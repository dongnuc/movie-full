using System;
using Microsoft.AspNetCore.SignalR;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time seat selection updates.
    ///
    /// Client kết nối với query param: /hubs/seat?showingId=&lt;guid&gt;
    ///
    /// OnConnectedAsync thực hiện 3 bước validation:
    ///   1. showingId phải có mặt và đúng format GUID.
    ///   2. Showing phải tồn tại trong DB và đang active (IsActive = true).
    ///   3. Showing chưa quá giờ chiếu (TimeStart > UtcNow).
    ///
    /// Nếu bất kỳ điều kiện nào thất bại → Context.Abort() để từ chối kết nối ngay lập tức.
    /// </summary>
    public class SeatHub : Hub
    {
        private readonly IShowingRepository _showingRepository;

        public SeatHub(IShowingRepository showingRepository)
        {
            _showingRepository = showingRepository;
        }

        /// <summary>
        /// Called when a client connects.
        /// Validates showingId then adds the connection to the Showing-scoped group.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var http       = Context.GetHttpContext();
            var rawId      = http?.Request.Query["showingId"].ToString();

            // ── 1. Kiểm tra showingId có mặt và đúng format GUID ──────────────
            if (string.IsNullOrWhiteSpace(rawId) || !Guid.TryParse(rawId, out var showingId))
            {
                Context.Abort();  // ngắt kết nối ngay, không throw
                return;
            }

            // ── 2 & 3. Kiểm tra DB: tồn tại, active, chưa quá giờ chiếu ───────
            var showing = await _showingRepository.GetByIdAsync(showingId);
            bool isValid = true;
            string? failReason = null;

            if (showing == null)
            {
                isValid = false;
                failReason = "Showing not found.";
            }
            else if (!showing.IsActive)
            {
                isValid = false;
                failReason = "Showing is inactive.";
            }
            else if (showing.TimeStart <= DateTime.UtcNow)
            {
                isValid = false;
                failReason = "Showing has already started.";
            }

            if (!isValid)
            {
                // Gửi thông báo lý do thất bại cho client trước khi abort
                // (client lắng nghe qua .onclose hoặc .catch của .start())
                await Clients.Caller.SendAsync("ConnectionRejected", failReason);
                Context.Abort();
                return;
            }

            // ── Hợp lệ: gán connection vào group scoped theo showingId ─────────
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(rawId));

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects.
        /// SignalR tự động xóa connection khỏi tất cả group khi disconnect,
        /// nhưng ta vẫn gọi tường minh để đảm bảo đúng nhóm.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var rawId = Context.GetHttpContext()?.Request.Query["showingId"].ToString();

            if (!string.IsNullOrWhiteSpace(rawId) && Guid.TryParse(rawId, out _))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(rawId));
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Naming convention cho SignalR Group theo showingId.
        /// Dùng chung bởi SeatHubService để đảm bảo nhất quán.
        /// </summary>
        public static string GetGroupName(string showingId) => $"showing-{showingId.ToLowerInvariant()}";
    }
}
