using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Queries.GetAllTheaters
{
    internal sealed class GetAllTheatersQueryHandler : IQueryHandler<GetAllTheatersQuery, PagedResult<TheaterResponse>>
    {
        private readonly ITheaterRepository _theaterRepository;

        public GetAllTheatersQueryHandler(ITheaterRepository theaterRepository)
        {
            _theaterRepository = theaterRepository;
        }

        public async Task<Result<PagedResult<TheaterResponse>>> Handle(
            GetAllTheatersQuery request,
            CancellationToken cancellationToken)
        {
            var pagedTheaters = await _theaterRepository.GetTheatersPagedAsync(
                request.Search,
                request.IsActive,
                request.Page,
                request.PageSize,
                cancellationToken);

            var items = pagedTheaters.Items.Select(TheaterMapper.ToResponse).ToList();

            var response = PagedResult<TheaterResponse>.Create(
                items,
                pagedTheaters.PageIndex,
                pagedTheaters.PageSize,
                pagedTheaters.TotalCount);

            return Result.Success(response);
        }
    }
}
