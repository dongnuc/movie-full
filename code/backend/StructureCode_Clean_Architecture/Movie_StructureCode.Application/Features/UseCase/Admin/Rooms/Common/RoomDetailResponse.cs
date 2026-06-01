using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common
{
    public sealed record RoomDetailResponse(
        Guid Id,
        string Name,
        int TotalSeat,
        int TotalRow,
        int TotalCol,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
