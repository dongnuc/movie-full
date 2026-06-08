using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Application.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Commands.UpdateRoom
{
    internal sealed class UpdateRoomCommandHandler : ICommandHander<UpdateRoomCommand>
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ITheaterRepository _theaterRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IShowingRepository _showingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public UpdateRoomCommandHandler(
            IRoomRepository roomRepository,
            ITheaterRepository theaterRepository,
            ISeatRepository seatRepository,
            IShowingRepository showingRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _roomRepository = roomRepository;
            _theaterRepository = theaterRepository;
            _seatRepository = seatRepository;
            _showingRepository = showingRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> Handle(
            UpdateRoomCommand request,
            CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(request.Id, cancellationToken);
            if (room == null)
            {
                return Result.Failure(RoomErrors.NotFound(request.Id));
            }

            var theater = await _theaterRepository.GetByIdAsync(request.TheaterId, cancellationToken);
            if (theater == null)
            {
                return Result.Failure(TheaterErrors.NotFound(request.TheaterId));
            }

            var nameExists = await _roomRepository.AnyAsync(
                r => r.TheaterId == request.TheaterId && r.Name == request.Name && r.Id != request.Id,
                cancellationToken);

            if (nameExists)
            {
                return Result.Failure(RoomErrors.NameAlreadyExists);
            }

            if(room.TotalRow != request.TotalRow || room.TotalCol != request.TotalCol){
                 var seatsExistInRoom = await _seatRepository.AnyAsync(s => s.RoomId == request.Id, cancellationToken);
                 if(seatsExistInRoom){
                    return Result.Failure(RoomErrors.SeatsExistInRoom);
                 }  
            }
                     

            var existShowing = await _showingRepository.AnyAsync(s => s.RoomId == request.Id
                && s.TimeStart > DateTime.UtcNow, cancellationToken);
            
            if(existShowing){
                return Result.Failure(RoomErrors.ShowingExistInRoom);
            }

            var totalSeat = request.TotalRow * request.TotalCol;
            room.UpdateDetails(
                request.Name,
                totalSeat,
                request.TotalRow,
                request.TotalCol,
                request.TheaterId,
                request.IsActive);

            _roomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache by publishing EntityChangedEvent
            await _publisher.Publish(new EntityChangedEvent("Room", room.Id), cancellationToken);

            return Result.Success();
        }
    }
}
