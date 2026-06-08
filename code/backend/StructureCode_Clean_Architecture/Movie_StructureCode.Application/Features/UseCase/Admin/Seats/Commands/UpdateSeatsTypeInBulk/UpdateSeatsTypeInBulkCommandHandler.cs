using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Movie_StructureCode.Application.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsTypeInBulk
{
    internal sealed class UpdateSeatsTypeInBulkCommandHandler : ICommandHander<UpdateSeatsTypeInBulkCommand>
    {
        private readonly ISeatRepository _seatRepository;
        private readonly ISeatTypeRepository _seatTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public UpdateSeatsTypeInBulkCommandHandler(
            ISeatRepository seatRepository,
            ISeatTypeRepository seatTypeRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _seatRepository = seatRepository;
            _seatTypeRepository = seatTypeRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> Handle(
            UpdateSeatsTypeInBulkCommand request,
            CancellationToken cancellationToken)
        {
            var seatType = await _seatTypeRepository.GetByIdAsync(
                request.SeatTypeId,
                cancellationToken);

            if (seatType is null || !seatType.IsActive)
            {
                return Result.Failure(SeatTypeErrors.NotFound(request.SeatTypeId));
            }

            if (seatType.Name.Contains("COUPLE", StringComparison.CurrentCultureIgnoreCase))
            {
                return Result.Failure(SeatErrors.CoupleSeatCannotBeUpdated);
            }


            var seats = await _seatRepository.FindAsync(s => request.SeatIds.Contains(s.Id), cancellationToken);
            var seatList = seats.ToList();

            if (seatList.Count != request.SeatIds.Count)
            {
                return Result.Failure(SeatErrors.SomeSeatsNotFound);
            }

            foreach (var seat in seatList)
            {
                seat.UpdateSeatType(request.SeatTypeId);
                seat.UpdateGroupId(null);
                _seatRepository.Update(seat);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache for each unique room affected
            var roomIds = seatList.Select(s => s.RoomId).Distinct();
            foreach (var roomId in roomIds)
            {
                await _publisher.Publish(new EntityChangedEvent("Room", roomId), cancellationToken);
            }

            return Result.Success();
        }
    }
}
