using System;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingInRoom
{
    public sealed record GetShowingInRoomQuery(
        Guid TheaterId,
        DateOnly? DateStart,
        Guid? RoomId,
        int PageIndex,
        int PageSize,
        bool? IsActive
    ) : IQuery<PagedResult<ShowingGroupedRoom>>;
}
