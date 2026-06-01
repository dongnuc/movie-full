using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMovieDetailUser
{
    internal sealed class GetMovieDetailUserQueryHandler : IQueryHandler<GetMovieDetailUserQuery, MovieDetailUserResponse>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IShowingRepository _showingRepository;

        public GetMovieDetailUserQueryHandler(
            IMovieRepository movieRepository,
            IShowingRepository showingRepository)
        {
            _movieRepository = movieRepository;
            _showingRepository = showingRepository;
        }

        public async Task<Result<MovieDetailUserResponse>> Handle(
            GetMovieDetailUserQuery request,
            CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetActiveMovieWithCategoryAsync(request.MovieId, cancellationToken);
            if (movie is null)
                return Result.Failure<MovieDetailUserResponse>(new Error("Movie.NotFound", "Phim không tồn tại hoặc đã bị vô hiệu hóa."));

            var showings = await _showingRepository.GetActiveShowingsForMovieTodayAsync(request.MovieId, cancellationToken);

            var theaterGroups = showings
                .GroupBy(s => new { s.Room!.TheaterId, TheaterName = s.Room!.Theater!.Name })
                .Select(g => new TheaterShowingGroup(
                    g.Key.TheaterId,
                    g.Key.TheaterName,
                    g.Count()))
                .ToList();

            var response = new MovieDetailUserResponse(
                movie.Id,
                movie.Title,
                movie.Description,
                movie.Image,
                movie.BasePrice,
                movie.Duration,
                movie.CategoryId,
                movie.Category?.Name ?? string.Empty,
                theaterGroups);

            return Result.Success(response);
        }
    }
}
