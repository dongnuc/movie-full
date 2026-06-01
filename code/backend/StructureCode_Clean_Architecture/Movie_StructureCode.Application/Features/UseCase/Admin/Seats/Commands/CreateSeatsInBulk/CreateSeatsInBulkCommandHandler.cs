using MediatR;
using Movie_StructureCode.Application.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;


namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.CreateSeatsInBulk
{
    internal sealed class CreateSeatsInBulkCommandHandler : ICommandHander<CreateSeatsInBulkCommand>
    {
        private readonly ISeatRepository _seatRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly ISeatTypeRepository _seatTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateSeatsInBulkCommandHandler(
            ISeatRepository seatRepository,
            IRoomRepository roomRepository,
            ISeatTypeRepository seatTypeRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _seatRepository = seatRepository;
            _roomRepository = roomRepository;
            _seatTypeRepository = seatTypeRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> Handle(
            CreateSeatsInBulkCommand request,
            CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetById(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure(RoomErrors.NotFound(request.RoomId));
            }

            var seatTypes = await _seatTypeRepository.FindAsync(x => true, cancellationToken);
            var defaultSeatType = seatTypes.FirstOrDefault();
            if (defaultSeatType == null)
            {
                return Result.Failure(SeatErrors.DefaultSeatTypeNotFound);
            }

            // Remove existing seats if any
            var existingSeats = await _seatRepository.FindAsync(s => s.RoomId == request.RoomId, cancellationToken);
            var existingSeatList = existingSeats.ToList();
            if (existingSeatList.Any())
            {
                _seatRepository.RemoveRange(existingSeatList);
            }

            var seatsToCreate = new List<Seat>();
            for (int r = 1; r <= room.TotalRow; r++)
            {
                string rowName = GetRowName(r);
                for (int c = 1; c <= room.TotalCol; c++)
                {
                    var seat = Seat.Create(
                        id: Guid.NewGuid(),
                        rowIndex: r,
                        colIndex: c,
                        rowName: rowName,
                        seatNumber: c,
                        groupId: null,
                        seatTypeId: defaultSeatType.Id,
                        roomId: room.Id
                    );
                    seatsToCreate.Add(seat);
                }
            }

            await _seatRepository.AddRangeAsync(seatsToCreate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate room cache
            // await _publisher.Publish(new EntityChangedEvent("Room", room.Id), cancellationToken);

            return Result.Success();
        }

        private static string GetRowName(int rowIndex)
        {
            string rowName = string.Empty;
            while (rowIndex > 0)
            {
                int modulo = (rowIndex - 1) % 26;
                rowName = Convert.ToChar(65 + modulo) + rowName;
                rowIndex = (rowIndex - modulo) / 26;
            }
            return rowName;
        }
    }
}
