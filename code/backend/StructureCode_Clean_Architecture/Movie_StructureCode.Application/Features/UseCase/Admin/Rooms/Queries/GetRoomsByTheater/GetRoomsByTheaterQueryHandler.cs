using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.GetRoomsByTheater
{
    internal sealed class GetRoomsByTheaterQueryHandler : IQueryHandler<GetRoomsByTheaterQuery, IEnumerable<RoomMinimalResponse>>
    {
        private readonly IRoomRepository _roomRepository;

        public GetRoomsByTheaterQueryHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<Result<IEnumerable<RoomMinimalResponse>>> Handle(
            GetRoomsByTheaterQuery request,
            CancellationToken cancellationToken)
        {
            var rooms = await _roomRepository.FindAsync(
                r => r.TheaterId == request.TheaterId && r.IsActive,
                cancellationToken);

            var response = rooms.Select(r => new RoomMinimalResponse(
                r.Id,
                r.Name,
                $"{r.TotalRow}x{r.TotalCol}"
            )).ToList();

            return Result.Success<IEnumerable<RoomMinimalResponse>>(response);
        }
    }
}
