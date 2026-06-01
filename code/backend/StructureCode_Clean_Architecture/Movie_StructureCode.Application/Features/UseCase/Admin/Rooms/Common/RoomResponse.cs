using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common
{
    public sealed record RoomResponse(
        Guid Id,
        string Name,
        int TotalSeat,
        string Matrix,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
