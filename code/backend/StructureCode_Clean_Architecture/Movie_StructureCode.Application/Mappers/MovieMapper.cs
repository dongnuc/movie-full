using Movie_StructureCode.Application.Abstractions.Services.Storage;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Mappers
{
    public static class MovieMapper
    {
        public static MovieResponse ToResponse(Movie movie, IStorageService storageService)
        {
            var movieIdStr = movie.Id.ToString();
            return new MovieResponse(
                movie.Id,
                movie.Title,
                storageService.GetAbsoluteUrl(movie.ImagePoster, movieIdStr),
                movie.IsHomePage,
                movie.BasePrice,
                movie.Duration,
                movie.Category?.Name ?? string.Empty,
                movie.IsActive,
                movie.DateCreate,
                movie.DateUpdate);
        }

        public static MovieDetailResponse ToDetailResponse(Movie movie, IStorageService storageService)
        {
            var movieIdStr = movie.Id.ToString();
            return new MovieDetailResponse(
                movie.Id,
                movie.Title,
                movie.Description,
                storageService.GetAbsoluteUrl(movie.ImageBanner, movieIdStr),
                storageService.GetAbsoluteUrl(movie.ImagePoster, movieIdStr),
                movie.IsHomePage,
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
