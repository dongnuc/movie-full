using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Queries.GetTheaterById
{
    internal sealed class GetTheaterByIdQueryHandler : IQueryHandler<GetTheaterByIdQuery, TheaterResponse>
    {
        private readonly ITheaterRepository _theaterRepository;

        public GetTheaterByIdQueryHandler(ITheaterRepository theaterRepository)
        {
            _theaterRepository = theaterRepository;
        }

        public async Task<Result<TheaterResponse>> Handle(
            GetTheaterByIdQuery request,
            CancellationToken cancellationToken)
        {
            var theater = await _theaterRepository.GetByIdAsync(request.Id, cancellationToken);
            if (theater == null)
            {
                return Result.Failure<TheaterResponse>(TheaterErrors.NotFound(request.Id));
            }

            return Result.Success(TheaterMapper.ToResponse(theater));
        }
    }
}
