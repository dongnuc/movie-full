using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;
using Movie = Movie_StructureCode.Domain.Entities.Movie;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.DeleteMovie
{
    internal sealed class DeleteMovieCommandHandler : ICommandHander<DeleteMovieCommand>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMovieCommandHandler(IMovieRepository movieRepository, IUnitOfWork unitOfWork)
        {
            _movieRepository = movieRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeleteMovieCommand request,
            CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);
            if (movie == null)
            {
                return Result.Failure(MovieErrors.NotFound(request.Id));
            }

            _movieRepository.Remove(movie);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
