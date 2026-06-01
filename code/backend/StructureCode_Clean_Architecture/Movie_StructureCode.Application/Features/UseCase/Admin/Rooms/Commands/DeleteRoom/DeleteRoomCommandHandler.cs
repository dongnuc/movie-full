using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Application.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.DeleteRoom
{
    internal sealed class DeleteRoomCommandHandler : ICommandHander<DeleteRoomCommand>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public DeleteRoomCommandHandler(
            IRoomRepository roomRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> Handle(
            DeleteRoomCommand request,
            CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(request.Id, cancellationToken);
            if (room == null)
            {
                return Result.Failure(RoomErrors.NotFound(request.Id));
            }

            _roomRepository.Remove(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache by publishing EntityChangedEvent
            await _publisher.Publish(new EntityChangedEvent("Room", room.Id), cancellationToken);

            return Result.Success();
        }
    }
}
