using System.Collections.Generic;

namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    public static class PermissionConstants
    {
        public const string Movie = "MOVIE";
        public const string Ticket = "TICKET";
        public const string Staff = "STAFF";
        public const string Showing = "SHOWING";
        public const string Theater = "THEATER";
        public const string Room = "ROOM";
        public const string Category = "CATEGORY";
        public const string Seat = "SEAT";
        public const string SeatType = "SEATTYPE";
        public const string Account = "ACCOUNT";
        public const string Role = "ROLE";
        public const string Booking = "BOOKING";

        public static readonly IReadOnlyCollection<string> AllCategories = new[]
        {
            Movie,
            Ticket,
            Staff,
            Showing,
            Theater,
            Room,
            Category,
            Seat,
            SeatType,
            Account,
            Role,
            Booking
        };
    }
}
