using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Queries.CheckSeatsExist
{
    internal sealed class CheckSeatsExistQueryHandler : IQueryHandler<CheckSeatsExistQuery, bool>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ISeatRepository _seatRepository;

        public CheckSeatsExistQueryHandler(
            IRoomRepository roomRepository,
            ISeatRepository seatRepository)
        {
            _roomRepository = roomRepository;
            _seatRepository = seatRepository;
        }

        public async Task<Result<bool>> Handle(
            CheckSeatsExistQuery request,
            CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetById(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<bool>(RoomErrors.NotFound(request.RoomId));
            }

            var hasSeats = await _seatRepository.AnyAsync(s => s.RoomId == request.RoomId, cancellationToken);
            return Result.Success(hasSeats);
        }
    }
}
