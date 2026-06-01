using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetAllRooms
{
    public sealed record GetAllRoomsQuery(
        string? Search,
        bool? IsActive,
        Guid TheaterId,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<RoomResponse>>;
}
