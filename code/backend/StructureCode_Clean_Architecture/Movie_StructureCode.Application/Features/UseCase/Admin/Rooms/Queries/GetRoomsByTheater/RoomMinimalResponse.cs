using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomsByTheater
{
    public sealed record RoomMinimalResponse(
        Guid Id,
        string Name,
        string Matrix);
}
