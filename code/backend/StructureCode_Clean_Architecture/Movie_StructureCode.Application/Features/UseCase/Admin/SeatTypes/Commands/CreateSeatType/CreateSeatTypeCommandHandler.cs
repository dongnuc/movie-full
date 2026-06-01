using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.CreateSeatType
{
    internal sealed class CreateSeatTypeCommandHandler : ICommandHander<CreateSeatTypeCommand>
    {
        private readonly ISeatTypeRepository _seatTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateSeatTypeCommandHandler(ISeatTypeRepository seatTypeRepository, IUnitOfWork unitOfWork)
        {
            _seatTypeRepository = seatTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            CreateSeatTypeCommand request,
            CancellationToken cancellationToken)
        {
            var nameExists = await _seatTypeRepository.ExistsByNameAsync(
                request.Name,
                propertyName: "Name",
                excludeId: null,
                ct: cancellationToken);

            if (nameExists)
            {
                return Result.Failure(SeatTypeErrors.NameAlreadyExists);
            }

            var seatType = SeatType.Create(
                Guid.NewGuid(),
                request.Name.ToUpper(),
                request.Surcharge,
                request.IsActive);

            await _seatTypeRepository.AddAsync(seatType, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
