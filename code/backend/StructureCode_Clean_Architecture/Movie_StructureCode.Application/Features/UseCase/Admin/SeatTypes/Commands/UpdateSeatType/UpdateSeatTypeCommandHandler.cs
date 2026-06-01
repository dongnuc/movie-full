using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;
using SeatType = Movie_StructureCode.Domain.Entities.SeatType;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.UpdateSeatType
{
    internal sealed class UpdateSeatTypeCommandHandler : ICommandHander<UpdateSeatTypeCommand>
    {
        private readonly ISeatTypeRepository _seatTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSeatTypeCommandHandler(ISeatTypeRepository seatTypeRepository, IUnitOfWork unitOfWork)
        {
            _seatTypeRepository = seatTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UpdateSeatTypeCommand request,
            CancellationToken cancellationToken)
        {
            var seatType = await _seatTypeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (seatType == null)
            {
                return Result.Failure(SeatTypeErrors.NotFound(request.Id));
            }

            var nameExists = await _seatTypeRepository.ExistsByNameAsync(
                request.Name,
                propertyName: "Name",
                excludeId: request.Id,
                ct: cancellationToken);

            if (nameExists)
            {
                return Result.Failure(SeatTypeErrors.NameAlreadyExists);
            }

            seatType.UpdateDetails(request.Name, request.Surcharge, request.IsActive);

            _seatTypeRepository.Update(seatType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
