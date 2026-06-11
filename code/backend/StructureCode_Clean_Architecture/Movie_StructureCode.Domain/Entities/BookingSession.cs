using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Domain.Entities
{
    public class BookingSession : BaseEntities
    {
        public Guid ShowingId { get; private set; }
        public Showing? Showing { get; private set; }

        public Guid AppUserId { get; private set; }
        public AppUser? AppUser { get; private set; }

        public string Status { get; private set; } = "select"; // select, paid, confirm, expired
        public DateTime ExpiredAt { get; private set; }
        public bool Extend { get; private set; } = false;
        public DateTime GracePeriodEndsAt { get; private set; }
        public DateTime? ReconnectedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public ICollection<BookingSessionSeat> BookingSessionSeats { get; private set; } = new List<BookingSessionSeat>();

        private BookingSession() : base() 
        {
            BookingSessionSeats = new List<BookingSessionSeat>();
        }

        private BookingSession(
            Guid id,
            Guid showingId,
            Guid appUserId,
            string status,
            DateTime createdAt,
            DateTime expiredAt,
            bool extend,
            DateTime gracePeriodEndsAt,
            DateTime? reconnectedAt,
            DateTime? cancelledAt,
            bool isActive)
            : base(id, createdAt, DateTime.UtcNow, isActive)
        {
            ShowingId = showingId;
            AppUserId = appUserId;
            Status = status;
            ExpiredAt = expiredAt;
            Extend = extend;
            GracePeriodEndsAt = gracePeriodEndsAt;
            ReconnectedAt = reconnectedAt;
            CancelledAt = cancelledAt;
            BookingSessionSeats = new List<BookingSessionSeat>();
        }

        public static BookingSession Create(
            Guid id,
            Guid showingId,
            Guid appUserId,
            string status = "select",
            int durationMinutes = 5)
        {
            var now = DateTime.UtcNow;
            var expiredAt = now.AddMinutes(durationMinutes);
            var gracePeriodEndsAt = expiredAt.AddSeconds(30);

            return new BookingSession(
                id,
                showingId,
                appUserId,
                status,
                now,
                expiredAt,
                extend: false,
                gracePeriodEndsAt,
                reconnectedAt: null,
                cancelledAt: null,
                isActive: true
            );
        }

        public void ChangeStatus(string status)
        {
            Status = status;
            DateUpdate = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = "expired";
            CancelledAt = DateTime.UtcNow;
            DateUpdate = DateTime.UtcNow;
        }

        public void Reconnect()
        {
            ReconnectedAt = DateTime.UtcNow;
            DateUpdate = DateTime.UtcNow;
        }

        public bool TryExtendSession(int extendMinutes = 2)
        {
            if (Extend) return false; // Already extended

            Extend = true;
            ExpiredAt = ExpiredAt.AddMinutes(extendMinutes);
            GracePeriodEndsAt = ExpiredAt.AddSeconds(30);
            Status = "paid"; // User goes to payment (paid status)
            DateUpdate = DateTime.UtcNow;
            return true;
        }
    }
}
