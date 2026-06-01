using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Commands.DeleteSeatType
{
    internal sealed class DeleteSeatTypeCommandHandler : ICommandHander<DeleteSeatTypeCommand>
    {
        private readonly ISeatTypeRepository _seatTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSeatTypeCommandHandler(ISeatTypeRepository seatTypeRepository, IUnitOfWork unitOfWork)
        {
            _seatTypeRepository = seatTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeleteSeatTypeCommand request,
            CancellationToken cancellationToken)
        {
            var seatType = await _seatTypeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (seatType == null)
            {
                return Result.Failure(SeatTypeErrors.NotFound(request.Id));
            }

            _seatTypeRepository.Remove(seatType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
