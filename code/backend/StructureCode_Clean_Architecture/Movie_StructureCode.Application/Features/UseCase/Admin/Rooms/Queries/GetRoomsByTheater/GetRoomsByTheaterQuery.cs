using System;
using System.Collections.Generic;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomsByTheater
{
    public sealed record GetRoomsByTheaterQuery(Guid TheaterId) : IQuery<IEnumerable<RoomMinimalResponse>>;
}
