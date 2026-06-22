using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Domain.Abstractions;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Queries.GetTheaterMembers
{
    internal sealed class GetTheaterMembersQueryHandler : IQueryHandler<GetTheaterMembersQuery, PagedResult<TheaterMemberResponse>>
    {
        private readonly ITheaterMemberQueryService _theaterMemberQueryService;

        public GetTheaterMembersQueryHandler(ITheaterMemberQueryService theaterMemberQueryService)
        {
            _theaterMemberQueryService = theaterMemberQueryService;
        }

        public async Task<Result<PagedResult<TheaterMemberResponse>>> Handle(
            GetTheaterMembersQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _theaterMemberQueryService.GetMembersPagedAsync(
                request.TheaterId,
                request.Search,
                request.Page,
                request.PageSize,
                cancellationToken);

            return Result.Success(result);
        }
    }
}
