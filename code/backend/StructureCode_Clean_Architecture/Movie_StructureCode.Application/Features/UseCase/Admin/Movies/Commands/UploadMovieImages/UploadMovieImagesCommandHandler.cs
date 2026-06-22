using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.Storage;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;
using SixLabors.ImageSharp;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.UploadMovieImages
{
    internal sealed class UploadMovieImagesCommandHandler : ICommandHander<UploadMovieImagesCommand>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;

        public UploadMovieImagesCommandHandler(
            IMovieRepository movieRepository,
            IStorageService storageService,
            IUnitOfWork unitOfWork)
        {
            _movieRepository = movieRepository;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UploadMovieImagesCommand request,
            CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);
            if (movie == null)
            {
                return Result.Failure(MovieErrors.NotFound(request.MovieId));
            }

            string movieIdStr = request.MovieId.ToString();

            // 1. Process Banner Image
            if (request.BannerStream != null && !string.IsNullOrEmpty(request.BannerFileName))
            {
                try
                {
                    using var bannerMs = new MemoryStream();
                    await request.BannerStream.CopyToAsync(bannerMs, cancellationToken);
                    bannerMs.Position = 0;

                    using (var image = await Image.LoadAsync(bannerMs, cancellationToken))
                    {
                        if (image.Width < 1600 || image.Height < 500 || image.Width <= image.Height)
                        {
                            return Result.Failure(new Error("Movie.InvalidBannerDimensions", "Ảnh banner phải là ảnh ngang (Landscape) và có kích thước tối thiểu là 1600x500."));
                        }
                    }

                    bannerMs.Position = 0;
                    var sanitizedBannerName = GetSanitizedFileName(request.BannerFileName);

                    await _storageService.UploadFileAsync(
                        bannerMs, 
                        sanitizedBannerName, 
                        movieIdStr, 
                        request.BannerContentType ?? "image/jpeg", 
                        cancellationToken);

                    movie.UpdateBanner(sanitizedBannerName);
                }
                catch (Exception ex)
                {
                    return Result.Failure(new Error("Movie.BannerUploadFailed", $"Upload banner thất bại: {ex.Message}"));
                }
            }

            // 2. Process Poster Image
            if (request.PosterStream != null && !string.IsNullOrEmpty(request.PosterFileName))
            {
                try
                {
                    using var posterMs = new MemoryStream();
                    await request.PosterStream.CopyToAsync(posterMs, cancellationToken);
                    posterMs.Position = 0;

                    using (var image = await Image.LoadAsync(posterMs, cancellationToken))
                    {
                        if (image.Width < 400 || image.Height < 800 || image.Height <= image.Width)
                        {
                            return Result.Failure(new Error("Movie.InvalidPosterDimensions", "Ảnh poster phải là ảnh dọc (Portrait) và có kích thước tối thiểu là 400x800."));
                        }
                    }

                    posterMs.Position = 0;
                    var sanitizedPosterName = GetSanitizedFileName(request.PosterFileName);

                    await _storageService.UploadFileAsync(
                        posterMs, 
                        sanitizedPosterName, 
                        movieIdStr, 
                        request.PosterContentType ?? "image/jpeg", 
                        cancellationToken);

                    movie.UpdatePoster(sanitizedPosterName);
                }
                catch (Exception ex)
                {
                    return Result.Failure(new Error("Movie.PosterUploadFailed", $"Upload poster thất bại: {ex.Message}"));
                }
            }

            _movieRepository.Update(movie);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        private static string GetSanitizedFileName(string originalFileName)
        {
            var fileNameWithoutPath = Path.GetFileName(originalFileName);
            var sanitized = string.Join("_", fileNameWithoutPath.Split(Path.GetInvalidFileNameChars()));
            sanitized = sanitized.Replace(" ", "_");
            return sanitized;
        }
    }
}
