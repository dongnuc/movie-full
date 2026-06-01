namespace Movie_StructureCode.Domain.Entities
{
    public enum BookingDetailStatus
    {
        Active,
        Refunded
    }
    
    public class BookingDetail : BaseEntities
    {
        public decimal Price { get; private set; }
        public BookingDetailStatus BookingDetailStatus { get; private set; } = BookingDetailStatus.Active;

        // foreign key

        public Guid BookingId { get; private set; }
        public Booking? Booking { get; private set; }
        public Guid ShowingSeatId { get; private set; }
        public ShowingSeat? ShowingSeat { get; private set; }

        private BookingDetail() : base() { }

        private BookingDetail(Guid id, decimal price, BookingDetailStatus bookingDetailStatus, Guid bookingId, Guid showingSeatId, bool isActive, DateTime dateCreate, DateTime dateUpdate)
            : base(id, dateCreate, dateUpdate, isActive)
        {
            Price = price;
            BookingDetailStatus = bookingDetailStatus;
            BookingId = bookingId;
            ShowingSeatId = showingSeatId;
        }

        public static BookingDetail Create(Guid id, decimal price, BookingDetailStatus bookingDetailStatus, Guid bookingId, Guid showingSeatId, bool isActive = true)
        {
            return new BookingDetail(
                id,
                price,
                bookingDetailStatus,
                bookingId,
                showingSeatId,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public void Refund()
        {
            BookingDetailStatus = BookingDetailStatus.Refunded;
            DateUpdate = DateTime.UtcNow;
        }
    }
}
