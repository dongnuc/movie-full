using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;
using Movie = Movie_StructureCode.Domain.Entities.Movie;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Queries.GetMovieDetailAdmin
{
    internal sealed class GetMovieDetailAdminQueryHandler : IQueryHandler<GetMovieDetailAdminQuery, MovieDetailResponse>
    {
        private readonly IMovieRepository _movieRepository;

        public GetMovieDetailAdminQueryHandler(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<Result<MovieDetailResponse>> Handle(
            GetMovieDetailAdminQuery request,
            CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdWithCategoryAsync(request.Id, cancellationToken);
            if (movie == null)
            {
                return Result.Failure<MovieDetailResponse>(MovieErrors.NotFound(request.Id));
            }

            var response = MovieMapper.ToDetailResponse(movie);
            return Result.Success(response);
        }
    }
}
