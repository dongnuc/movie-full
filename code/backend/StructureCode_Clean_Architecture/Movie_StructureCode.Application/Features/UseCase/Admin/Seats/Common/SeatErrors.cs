using System;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Common
{
    public static class SeatErrors
    {
        public static Error NotFound(Guid id) =>
            new("Seat.NotFound", $"Seat with ID {id} was not found.");

        public static readonly Error SomeSeatsNotFound =
            new("Seat.SomeSeatsNotFound", "One or more specified seats were not found.");

        public static readonly Error DefaultSeatTypeNotFound =
            new("Seat.DefaultSeatTypeNotFound", "No default SeatType found in the database. Seats cannot be generated.");

        public static readonly Error CoupleSeatsCountMustBeEven =
            new("Seat.CoupleSeatsCountMustBeEven", "Số lượng ghế được chọn phải là số chẵn.");

        public static readonly Error CoupleSeatsMustBeAdjacent =
            new("Seat.CoupleSeatsMustBeAdjacent", "Các cặp ghế đôi được chọn phải cùng một hàng và nằm cạnh nhau.");

        public static readonly Error CoupleSeatTypeNotFound =
            new("Seat.CoupleSeatTypeNotFound", "Không tìm thấy loại ghế Couple hoạt động trong hệ thống.");

        public static readonly Error CoupleSeatCannotBeUpdated =
            new("Seat.CoupleSeatCannotBeUpdated", "Không thể cập nhật loại ghế Couple.");
    }
}
