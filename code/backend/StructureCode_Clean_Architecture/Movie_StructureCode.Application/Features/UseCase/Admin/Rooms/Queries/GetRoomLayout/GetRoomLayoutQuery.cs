using System;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomLayout
{
    public sealed record GetRoomLayoutQuery(Guid Id) : IQuery<RoomLayoutResponse>, ICacheableQuery
    {
        public string CacheKey => "Room:RoomLayout";

        public TimeSpan? Expiration => TimeSpan.FromDays(1);

        public bool BypassCache => false;

        public Type ValueType => typeof(RoomLayoutResponse);

    }
}
