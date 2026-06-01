using System;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common
{
    public static class ShowingErrors
    {
        public static Error NotFound(Guid id) =>
            new("Showing.NotFound", $"Lịch chiếu với ID {id} không tìm thấy.");

        public static readonly Error RoomConflict =
            new("Showing.RoomConflict", "Lịch chiếu bị trùng lặp thời gian trong phòng chiếu này.");

        public static readonly Error MovieNotFound =
            new("Showing.MovieNotFound", "Không tìm thấy bộ phim được chọn.");

        public static readonly Error RoomNotFound =
            new("Showing.RoomNotFound", "Không tìm thấy phòng chiếu được chọn.");
    }
}
