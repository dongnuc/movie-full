using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomById
{
    internal sealed class GetRoomByIdQueryHandler : IQueryHandler<GetRoomByIdQuery, RoomDetailResponse>
    {
        private readonly IRoomRepository _roomRepository;

        public GetRoomByIdQueryHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<Result<RoomDetailResponse>> Handle(
            GetRoomByIdQuery request,
            CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetById(request.Id, cancellationToken);
            if (room == null)
            {
                return Result.Failure<RoomDetailResponse>(RoomErrors.NotFound(request.Id));
            }

            var response = RoomMapper.ToDetailResponse(room);
            return Result.Success(response);
        }
    }
}
