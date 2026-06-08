using System;
using Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetShowingSeatsLayout
{
    public sealed record GetShowingSeatsLayoutQuery(
        Guid ShowingId,
        Guid RoomId,
        Guid? CurrentUserId
    ) : IQuery<UserRoomLayoutResponse>;
}
