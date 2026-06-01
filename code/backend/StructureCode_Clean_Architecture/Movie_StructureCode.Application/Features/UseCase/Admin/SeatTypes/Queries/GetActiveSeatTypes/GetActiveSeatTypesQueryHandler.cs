using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetActiveSeatTypes
{
    internal sealed class GetActiveSeatTypesQueryHandler : IQueryHandler<GetActiveSeatTypesQuery, IEnumerable<SeatTypeActiveResponse>>
    {
        private readonly ISeatTypeRepository _seatTypeRepository;

        public GetActiveSeatTypesQueryHandler(ISeatTypeRepository seatTypeRepository)
        {
            _seatTypeRepository = seatTypeRepository;
        }

        public async Task<Result<IEnumerable<SeatTypeActiveResponse>>> Handle(
            GetActiveSeatTypesQuery request,
            CancellationToken cancellationToken)
        {
            var seatTypes = await _seatTypeRepository.GetActiveSeatTypesAsync(cancellationToken);
            var response = seatTypes.Select(SeatTypeMapper.ToActiveResponse).ToList();
            
            return Result.Success<IEnumerable<SeatTypeActiveResponse>>(response);
        }
    }
}
