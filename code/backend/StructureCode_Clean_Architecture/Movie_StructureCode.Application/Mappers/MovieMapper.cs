using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Mappers
{
    public static class MovieMapper
    {
        public static MovieResponse ToResponse(Movie movie)
        {
            return new MovieResponse(
                movie.Id,
                movie.Title,
                movie.Image,
                movie.BasePrice,
                movie.Duration,
                movie.Category?.Name ?? string.Empty,
                movie.IsActive,
                movie.DateCreate,
                movie.DateUpdate);
        }

        public static MovieDetailResponse ToDetailResponse(Movie movie)
        {
            return new MovieDetailResponse(
                movie.Id,
                movie.Title,
                movie.Description,
                movie.Image,
                movie.BasePrice,
                movie.Duration,
                movie.CategoryId,
                movie.Category?.Name ?? string.Empty,
                movie.IsActive,
                movie.DateCreate,
                movie.DateUpdate);
        }
    }
}
