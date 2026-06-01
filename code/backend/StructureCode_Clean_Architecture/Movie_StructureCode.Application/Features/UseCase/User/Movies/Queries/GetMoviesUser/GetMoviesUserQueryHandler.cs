using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesUser
{
    internal sealed class GetMoviesUserQueryHandler : IQueryHandler<GetMoviesUserQuery, PagedResult<MovieUserResponse>>
    {
        private readonly IMovieRepository _movieRepository;

        public GetMoviesUserQueryHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<Result<PagedResult<MovieUserResponse>>> Handle(
            GetMoviesUserQuery request,
            CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetMoviesPagedAsync(
                request.Search,
                request.CategoryId,
                true, // isActive = true for user
                request.Page,
                request.PageSize,
                cancellationToken);

            var responseItems = movies.Items.Select(m => new MovieUserResponse(
                m.Id,
                m.Title,
                m.Image,
                m.Duration,
                m.BasePrice,
                m.CategoryId,
                m.Category?.Name ?? string.Empty)).ToList();

            var result = PagedResult<MovieUserResponse>.Create(
                responseItems,
                movies.PageIndex,
                movies.PageSize,
                movies.TotalCount);

            return Result.Success(result);
        }
    }
}
