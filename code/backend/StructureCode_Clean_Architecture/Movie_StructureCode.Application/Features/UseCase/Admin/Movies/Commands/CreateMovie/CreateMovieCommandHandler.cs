using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.CreateMovie
{
    internal sealed class CreateMovieCommandHandler : ICommandHander<CreateMovieCommand>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateMovieCommandHandler(
            IMovieRepository movieRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _movieRepository = movieRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            CreateMovieCommand request,
            CancellationToken cancellationToken)
        {
            // Verify Category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
            }

            // Check duplicate Title
            var titleExists = await _movieRepository.ExistsByNameAsync(request.Title, propertyName: "Title", excludeId: null, ct: cancellationToken);
            if (titleExists)
            {
                return Result.Failure(MovieErrors.TitleAlreadyExists);
            }

            var movie = Movie.Create(
                Guid.NewGuid(),
                request.Title,
                request.Description,
                request.Image,
                request.BasePrice,
                request.Duration,
                request.CategoryId,
                request.IsActive);

            await _movieRepository.AddAsync(movie, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
