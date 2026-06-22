using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;
using Movie = Movie_StructureCode.Domain.Entities.Movie;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.UpdateMovie
{
    internal sealed class UpdateMovieCommandHandler : ICommandHander<UpdateMovieCommand>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateMovieCommandHandler(
            IMovieRepository movieRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _movieRepository = movieRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UpdateMovieCommand request,
            CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);
            if (movie == null)
            {
                return Result.Failure(MovieErrors.NotFound(request.Id));
            }

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
            }

            var titleExists = await _movieRepository.ExistsByNameAsync(request.Title, propertyName: "Title", excludeId: request.Id, ct: cancellationToken);
            if (titleExists)
            {
                return Result.Failure(MovieErrors.TitleAlreadyExists);
            }

            if (request.IsHomePage)
            {
                var homePageExists = await _movieRepository.AnyOtherHomePageMovieExistsAsync(request.Id, cancellationToken);
                if (homePageExists)
                {
                    return Result.Failure(new Error("Movie.HomePageAlreadyExists", "Chỉ được phép cấu hình duy nhất một phim hiển thị trên Trang chủ."));
                }
            }

            movie.UpdateDetails(
                request.Title,
                request.Description,
                request.Image,
                request.BasePrice,
                request.Duration,
                request.CategoryId,
                request.IsActive,
                imageBanner: null,
                imagePoster: null,
                isHomePage: request.IsHomePage);

            if (request.ClearBanner)
            {
                movie.UpdateBanner(null);
            }

            if (request.ClearPoster)
            {
                movie.UpdatePoster(null);
            }

            _movieRepository.Update(movie);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
