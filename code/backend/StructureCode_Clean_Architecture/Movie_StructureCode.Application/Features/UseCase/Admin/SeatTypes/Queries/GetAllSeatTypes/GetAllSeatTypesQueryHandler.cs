using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetAllSeatTypes
{
    internal sealed class GetAllSeatTypesQueryHandler : IQueryHandler<GetAllSeatTypesQuery, PagedResult<SeatTypeResponse>>
    {
        private readonly ISeatTypeRepository _seatTypeRepository;

        public GetAllSeatTypesQueryHandler(ISeatTypeRepository seatTypeRepository)
        {
            _seatTypeRepository = seatTypeRepository;
        }

        public async Task<Result<PagedResult<SeatTypeResponse>>> Handle(
            GetAllSeatTypesQuery request,
            CancellationToken cancellationToken)
        {
            var seatTypes = await _seatTypeRepository.GetAllAsync(
                request.Id,
                request.Page,
                request.PageSize,
                cancellationToken);

            var responseItems = seatTypes.Items.Select(SeatTypeMapper.ToResponse).ToList();
            var result = PagedResult<SeatTypeResponse>.Create(
                responseItems,
                seatTypes.PageIndex,
                seatTypes.PageSize,
                seatTypes.TotalCount);

            return Result.Success(result);
        }
    }
}
