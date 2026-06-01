using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common
{
    public sealed record ShowingResponse(
        Guid Id,
        DateTime TimeStart,
        decimal Price,
        int MaxTicketsPerUser,
        Guid MovieId,
        string MovieTitle,
        Guid RoomId,
        string RoomName,
        Guid TheaterId,
        bool IsActive);
}
