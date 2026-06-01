using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetSeatTypeById
{
    internal sealed class GetSeatTypeByIdQueryHandler : IQueryHandler<GetSeatTypeByIdQuery, SeatTypeResponse>
    {
        private readonly ISeatTypeRepository _seatTypeRepository;

        public GetSeatTypeByIdQueryHandler(ISeatTypeRepository seatTypeRepository)
        {
            _seatTypeRepository = seatTypeRepository;
        }

        public async Task<Result<SeatTypeResponse>> Handle(
            GetSeatTypeByIdQuery request,
            CancellationToken cancellationToken)
        {
            var seatType = await _seatTypeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (seatType == null)
            {
                return Result.Failure<SeatTypeResponse>(SeatTypeErrors.NotFound(request.Id));
            }

            var response = SeatTypeMapper.ToResponse(seatType);
            return Result.Success(response);
        }
    }
}
