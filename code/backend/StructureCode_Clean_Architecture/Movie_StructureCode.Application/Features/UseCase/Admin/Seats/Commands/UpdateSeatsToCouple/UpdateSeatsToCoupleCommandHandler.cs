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

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsToCouple
{
    internal sealed class UpdateSeatsToCoupleCommandHandler : ICommandHander<UpdateSeatsToCoupleCommand>
    {
        private readonly ISeatRepository _seatRepository;
        private readonly ISeatTypeRepository _seatTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public UpdateSeatsToCoupleCommandHandler(
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
            UpdateSeatsToCoupleCommand request,
            CancellationToken cancellationToken)
        {
            if (request.SeatIds == null || request.SeatIds.Count == 0)
            {
                return Result.Failure(SeatErrors.CoupleSeatsCountMustBeEven);
            }

            if (request.SeatIds.Count % 2 != 0)
            {
                return Result.Failure(SeatErrors.CoupleSeatsCountMustBeEven);
            }

            // Tìm loại ghế Couple đang hoạt động trong DB
            var seatTypes = await _seatTypeRepository.FindAsync(st => st.Name.ToUpper() == "COUPLE" && st.IsActive, cancellationToken);
            var coupleSeatType = seatTypes.FirstOrDefault();
            if (coupleSeatType == null)
            {
                return Result.Failure(SeatErrors.CoupleSeatTypeNotFound);
            }

            // Lấy danh sách các ghế cần update
            var seats = await _seatRepository.FindAsync(s => request.SeatIds.Contains(s.Id), cancellationToken);
            var seatList = seats.ToList();

            if (seatList.Count != request.SeatIds.Count)
            {
                return Result.Failure(SeatErrors.SomeSeatsNotFound);
            }

            // Nhóm theo hàng (RowIndex) để kiểm tra liền kề và cùng hàng
            var groupedByRow = seatList.GroupBy(s => s.RowIndex);

            foreach (var group in groupedByRow)
            {
                if (group.Count() % 2 != 0)
                {
                    return Result.Failure(SeatErrors.CoupleSeatsMustBeAdjacent);
                }

                // Sắp xếp theo ColIndex để kiểm tra liền kề
                var sortedSeats = group.OrderBy(s => s.ColIndex).ToList();

                for (int i = 0; i < sortedSeats.Count; i += 2)
                {
                    var seat1 = sortedSeats[i];
                    var seat2 = sortedSeats[i + 1];

                    // Kiểm tra liền kề
                    if (seat2.ColIndex - seat1.ColIndex != 1)
                    {
                        return Result.Failure(SeatErrors.CoupleSeatsMustBeAdjacent);
                    }
                }
            }

            // Validation pass -> Thực hiện update
            foreach (var group in groupedByRow)
            {
                var sortedSeats = group.OrderBy(s => s.ColIndex).ToList();

                for (int i = 0; i < sortedSeats.Count; i += 2)
                {
                    var seat1 = sortedSeats[i];
                    var seat2 = sortedSeats[i + 1];

                    // Tạo GroupId chung cho cặp đôi ghế này
                    var coupleGroupId = Guid.NewGuid();

                    seat1.UpdateSeatType(coupleSeatType.Id);
                    seat1.UpdateGroupId(coupleGroupId);

                    seat2.UpdateSeatType(coupleSeatType.Id);
                    seat2.UpdateGroupId(coupleGroupId);

                    _seatRepository.Update(seat1);
                    _seatRepository.Update(seat2);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate cache cho các Room có ghế thay đổi
            var affectedRoomIds = seatList.Select(s => s.RoomId).Distinct().ToList();
            // foreach (var roomId in affectedRoomIds)
            // {
            //     await _publisher.Publish(new EntityChangedEvent("Room", roomId), cancellationToken);
            // }

            return Result.Success();
        }
    }
}
