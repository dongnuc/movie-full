using System;

namespace Movie_StructureCode.Domain.Entities
{
    public class BookingSessionSeat
    {
        public Guid BookingSessionId { get; private set; }
        public BookingSession? BookingSession { get; private set; }

        public Guid SeatId { get; private set; }
        public Seat? Seat { get; private set; }

        private BookingSessionSeat() { }

        public BookingSessionSeat(Guid bookingSessionId, Guid seatId)
        {
            BookingSessionId = bookingSessionId;
            SeatId = seatId;
        }
    }
}
