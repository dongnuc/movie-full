namespace Movie_StructureCode.Domain.Entities
{
    public enum ShowingSeatStatus
    {
        Available,
        Locked,
        Booked
    }

    public class ShowingSeat : BaseEntities
    {
        public ShowingSeatStatus Status { get; private set; } = ShowingSeatStatus.Available;

        public decimal Price { get; private set; }

        /// <summary>
        /// Thời điểm hết hạn lock (dùng khi Status = Locked).
        /// Null nếu ghế đang Available hoặc đã Booked.
        /// </summary>
        public DateTime? LockedUntil { get; private set; }

        // ── Foreign keys ──────────────────────────────────────────────────────

        public Guid ShowingId { get; private set; }
        public Showing? Showing { get; private set; }

        public Guid SeatId { get; private set; }
        public Seat? Seat { get; private set; }

        /// <summary>
        /// Null khi ghế chưa được đặt (Available / Locked).
        /// Có giá trị khi Status = Booked.
        /// </summary>
        public Guid? BookingId { get; private set; }
        public Booking? Booking { get; private set; }

        public BookingDetail? BookingDetail { get; private set; }

        private ShowingSeat() : base() { }

        private ShowingSeat(
            Guid id,
            ShowingSeatStatus status,
            decimal price,
            DateTime? lockedUntil,
            Guid showingId,
            Guid seatId,
            Guid? bookingId,
            bool isActive,
            DateTime dateCreate,
            DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            Status = status;
            Price = price;
            LockedUntil = lockedUntil;
            ShowingId = showingId;
            SeatId = seatId;
            BookingId = bookingId;
        }

        public static ShowingSeat Create(
            Guid id,
            ShowingSeatStatus status,
            decimal price,
            DateTime? lockedUntil,
            Guid showingId,
            Guid seatId,
            Guid? bookingId = null,
            bool isActive = true)
        {
            return new ShowingSeat(
                id,
                status,
                price,
                lockedUntil,
                showingId,
                seatId,
                bookingId,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void Lock(DateTime lockedUntil)
        {
            Status = ShowingSeatStatus.Locked;
            LockedUntil = lockedUntil;
            BookingId = null;
            DateUpdate = DateTime.UtcNow;
        }

        public void Unlock()
        {
            Status = ShowingSeatStatus.Available;
            LockedUntil = null;
            BookingId = null;
            DateUpdate = DateTime.UtcNow;
        }

        public void Book(Guid bookingId)
        {
            Status = ShowingSeatStatus.Booked;
            LockedUntil = null;
            BookingId = bookingId;
            DateUpdate = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal price)
        {
            Price = price;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
