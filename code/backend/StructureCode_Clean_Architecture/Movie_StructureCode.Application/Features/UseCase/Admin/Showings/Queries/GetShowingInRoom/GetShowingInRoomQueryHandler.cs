using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingInRoom
{
    internal sealed class GetShowingInRoomQueryHandler : IQueryHandler<GetShowingInRoomQuery, PagedResult<ShowingGroupedRoom>>
    {
        private readonly IShowingRepository _showingRepository;

        public GetShowingInRoomQueryHandler(IShowingRepository showingRepository)
        {
            _showingRepository = showingRepository;
        }

        public async Task<Result<PagedResult<ShowingGroupedRoom>>> Handle(
            GetShowingInRoomQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _showingRepository.GetAllShowingGroupedRoom(
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
