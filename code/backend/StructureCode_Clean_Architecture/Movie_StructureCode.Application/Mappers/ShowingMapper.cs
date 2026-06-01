using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Mappers
{
    public static class ShowingMapper
    {
        public static ShowingResponse ToResponse(Showing showing)
        {
            return new ShowingResponse(
                showing.Id,
                showing.TimeStart,
                showing.Price,
                showing.MaxTicketsPerUser,
                showing.MovieId,
                showing.Movie?.Title ?? string.Empty,
                showing.RoomId,
                showing.Room?.Name ?? string.Empty,
                showing.TheaterId,
                showing.IsActive);
        }

        public static ShowingDetailResponse ToDetailResponse(Showing showing)
        {
            return new ShowingDetailResponse(
                showing.Id,
                showing.TimeStart,
                showing.Price,
                showing.MaxTicketsPerUser,
                showing.MovieId,
                showing.Movie?.Title ?? string.Empty,
                showing.Movie?.Duration ?? 0,
                showing.RoomId,
                showing.Room?.Name ?? string.Empty,
                showing.TheaterId,
                showing.IsActive,
                showing.DateCreate,
                showing.DateUpdate);
        }
    }
}
