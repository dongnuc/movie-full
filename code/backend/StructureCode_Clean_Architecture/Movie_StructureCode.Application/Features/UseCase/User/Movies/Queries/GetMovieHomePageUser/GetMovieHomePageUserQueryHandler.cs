using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.Storage;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMovieHomePageUser
{
    internal sealed class GetMovieHomePageUserQueryHandler : IQueryHandler<GetMovieHomePageUserQuery, MovieHomePageResponse>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IStorageService _storageService;

        public GetMovieHomePageUserQueryHandler(
            IMovieRepository movieRepository,
            IStorageService storageService)
        {
            _movieRepository = movieRepository;
            _storageService = storageService;
        }

        public async Task<Result<MovieHomePageResponse>> Handle(
            GetMovieHomePageUserQuery request,
            CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetActiveHomePageMovieAsync(cancellationToken);
            if (movie is null)
            {
                return Result.Failure<MovieHomePageResponse>(new Error("Movie.HomePageNotFound", "Không tìm thấy bộ phim nào cấu hình hiển thị Trang chủ."));
            }

            var movieIdStr = movie.Id.ToString();
            var response = new MovieHomePageResponse(
                movie.Id,
                movie.Title,
                movie.Description,
                _storageService.GetAbsoluteUrl(movie.ImageBanner, movieIdStr),
                _storageService.GetAbsoluteUrl(movie.ImagePoster, movieIdStr),
                movie.Duration,
                movie.BasePrice,
                movie.Category?.Name ?? string.Empty);

            return Result.Success(response);
        }
    }
}
