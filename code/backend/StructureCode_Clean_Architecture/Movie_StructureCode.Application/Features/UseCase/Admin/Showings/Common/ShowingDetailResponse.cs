using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common
{
    public sealed record ShowingDetailResponse(
        Guid Id,
        DateTime TimeStart,
        decimal Price,
        int MaxTicketsPerUser,
        Guid MovieId,
        string MovieTitle,
        int MovieDuration,
        Guid RoomId,
        string RoomName,
        Guid TheaterId,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
