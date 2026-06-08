using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.CheckSeatsExist
{
    public sealed record CheckSeatsExistQuery(Guid RoomId) : IQuery<bool>;
}
