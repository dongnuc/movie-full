using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Application.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.CreateRoom
{
    internal sealed class CreateRoomCommandHandler : ICommandHander<CreateRoomCommand>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ITheaterRepository _theaterRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateRoomCommandHandler(
            IRoomRepository roomRepository,
            ITheaterRepository theaterRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _roomRepository = roomRepository;
            _theaterRepository = theaterRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> Handle(
            CreateRoomCommand request,
            CancellationToken cancellationToken)
        {
            var theater = await _theaterRepository.GetByIdAsync(request.TheaterId, cancellationToken);
            if (theater == null)
            {
                return Result.Failure(TheaterErrors.NotFound(request.TheaterId));
            }

            var nameExists = await _roomRepository.AnyAsync(
                r => r.TheaterId == request.TheaterId && r.Name == request.Name,
                cancellationToken);

            if (nameExists)
            {
                return Result.Failure(RoomErrors.NameAlreadyExists);
            }

            var totalSeat = request.TotalRow * request.TotalCol;
            var room = Room.Create(
                Guid.NewGuid(),
                request.Name,
                totalSeat,
                request.TotalRow,
                request.TotalCol,
                request.TheaterId,
                request.IsActive);

            await _roomRepository.CreateRoom(room, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
