using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetAllRooms
{
    internal sealed class GetAllRoomsQueryHandler : IQueryHandler<GetAllRoomsQuery, PagedResult<RoomResponse>>
    {
        private readonly IRoomRepository _roomRepository;

        public GetAllRoomsQueryHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<Result<PagedResult<RoomResponse>>> Handle(
            GetAllRoomsQuery request,
            CancellationToken cancellationToken)
        {
            var rooms = await _roomRepository.GetAllRoom(
                request.Search,
                request.IsActive,
                request.TheaterId,
                request.Page,
                request.PageSize,
                cancellationToken);

            var mappedItems = rooms.Items.Select(RoomMapper.ToResponse).ToList();
            var result = PagedResult<RoomResponse>.Create(
                mappedItems,
                rooms.PageIndex,
                rooms.PageSize,
                rooms.TotalCount);

            return Result.Success(result);
        }
    }
}
