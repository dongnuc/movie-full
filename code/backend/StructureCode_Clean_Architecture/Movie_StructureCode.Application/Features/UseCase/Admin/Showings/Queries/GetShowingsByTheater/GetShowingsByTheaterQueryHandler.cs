using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingsByTheater
{
    internal sealed class GetShowingsByTheaterQueryHandler : IQueryHandler<GetShowingsByTheaterQuery, PagedResult<ShowingGroupedByMovie>>
    {
        private readonly IShowingRepository _showingRepository;

        public GetShowingsByTheaterQueryHandler(IShowingRepository showingRepository)
        {
            _showingRepository = showingRepository;
        }

        public async Task<Result<PagedResult<ShowingGroupedByMovie>>> Handle(
            GetShowingsByTheaterQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _showingRepository.GetAllShowing(
                request.TheaterId,
                request.DateStart,
                request.RoomId,
                request.PageIndex,
                request.PageSize,
                request.IsActive,
                cancellationToken);

            return Result.Success(result);
        }
    }
}
