using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomLayout
{
    public sealed class GetRoomLayoutQueryHandler : IQueryHandler<GetRoomLayoutQuery, RoomLayoutResponse>
    {
        private readonly IRoomRepository _roomRepository;
        

        public GetRoomLayoutQueryHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<Result<RoomLayoutResponse>> Handle(
            GetRoomLayoutQuery request,
            CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetLayoutRoom(request.Id, cancellationToken);
            if (room == null)
            {
                return Result.Failure<RoomLayoutResponse>(RoomErrors.NotFound(request.Id));
            }

            var response = RoomMapper.ToLayoutResponse(room);
            return Result.Success(response);
        }
    }
}
