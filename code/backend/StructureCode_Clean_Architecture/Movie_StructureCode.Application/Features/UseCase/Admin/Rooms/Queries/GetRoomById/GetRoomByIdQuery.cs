using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomById
{
    public sealed record GetRoomByIdQuery(Guid Id) : IQuery<RoomDetailResponse>;
}
