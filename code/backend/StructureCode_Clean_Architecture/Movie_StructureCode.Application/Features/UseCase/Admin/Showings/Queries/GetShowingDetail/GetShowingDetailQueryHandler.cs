using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingDetail
{
    internal sealed class GetShowingDetailQueryHandler : IQueryHandler<GetShowingDetailQuery, ShowingDetailResponse>
    {
        private readonly IShowingRepository _showingRepository;

        public GetShowingDetailQueryHandler(IShowingRepository showingRepository)
        {
            _showingRepository = showingRepository;
        }

        public async Task<Result<ShowingDetailResponse>> Handle(
            GetShowingDetailQuery request,
            CancellationToken cancellationToken)
        {
            var showing = await _showingRepository.GetShowingWithDetailsAsync(request.Id, cancellationToken);
            if (showing == null)
            {
                return Result.Failure<ShowingDetailResponse>(ShowingErrors.NotFound(request.Id));
            }

            var response = ShowingMapper.ToDetailResponse(showing);
            return Result.Success(response);
        }
    }
}
