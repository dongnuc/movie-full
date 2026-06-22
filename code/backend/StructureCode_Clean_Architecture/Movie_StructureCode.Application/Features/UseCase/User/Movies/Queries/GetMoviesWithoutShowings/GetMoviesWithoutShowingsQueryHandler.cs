using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.Storage;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesWithoutShowings
{
    internal sealed class GetMoviesWithoutShowingsQueryHandler : IQueryHandler<GetMoviesWithoutShowingsQuery, List<MovieUserResponse>>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IStorageService _storageService;

        public GetMoviesWithoutShowingsQueryHandler(IMovieRepository movieRepository, IStorageService storageService)
        {
            _movieRepository = movieRepository;
            _storageService = storageService;
        }

        public async Task<Result<List<MovieUserResponse>>> Handle(
            GetMoviesWithoutShowingsQuery request,
            CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetActiveMoviesWithoutShowingsAsync(10, cancellationToken);

            var response = movies.Select(m => {
                var movieIdStr = m.Id.ToString();
                return new MovieUserResponse(
                    m.Id,
                    m.Title,
                    _storageService.GetAbsoluteUrl(m.ImagePoster, movieIdStr),
                    m.IsHomePage,
                    m.Duration,
                    m.BasePrice,
                    m.CategoryId,
                    m.Category?.Name ?? string.Empty
                );
            }).ToList();

            return Result.Success(response);
        }
    }
}
