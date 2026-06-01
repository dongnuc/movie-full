namespace Movie_StructureCode.Domain.Entities
{
    public enum StatusBooking
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public class Booking : BaseEntities
    {
        public decimal TotalAmount { get; private set; }
        public StatusBooking StatusBooking { get; private set; } = StatusBooking.Pending;

        //foreign key

        public Guid AppUserId { get; private set; }
        public AppUser? AppUser { get; private set; }

        public Guid ShowingId { get; private set; }
        public Showing? Showing { get; private set; }

        public ICollection<BookingDetail>? BookingDetails { get; private set; }

        private Booking() : base() { }

        private Booking(Guid id, decimal totalAmount, StatusBooking statusBooking, Guid appUserId, Guid showingId, bool isActive, DateTime dateCreate, DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            TotalAmount = totalAmount;
            StatusBooking = statusBooking;
            AppUserId = appUserId;
            ShowingId = showingId;
        }

        public static Booking Create(Guid id, decimal totalAmount, StatusBooking statusBooking, Guid appUserId, Guid showingId, bool isActive = true)
        {
            return new Booking(
                id,
                totalAmount,
                statusBooking,
                appUserId,
                showingId,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void Confirm()
        {
            StatusBooking = StatusBooking.Confirmed;
            DateUpdate = DateTime.UtcNow;
        }

        public void Cancel()
        {
            StatusBooking = StatusBooking.Cancelled;
            DateUpdate = DateTime.UtcNow;
        }

        public void UpdateAmount(decimal totalAmount)
        {
            TotalAmount = totalAmount;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
