using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.UpdateTheater
{
    internal sealed class UpdateTheaterCommandHandler : ICommandHander<UpdateTheaterCommand>
    {
        private readonly ITheaterRepository _theaterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTheaterCommandHandler(ITheaterRepository theaterRepository, IUnitOfWork unitOfWork)
        {
            _theaterRepository = theaterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UpdateTheaterCommand request,
            CancellationToken cancellationToken)
        {
            var theater = await _theaterRepository.GetByIdAsync(request.Id, cancellationToken);
            if (theater == null)
            {
                return Result.Failure(TheaterErrors.NotFound(request.Id));
            }

            // Check duplicate name for other theaters
            var nameExists = await _theaterRepository.ExistsByNameAsync(request.Name, propertyName: null, excludeId: request.Id, ct: cancellationToken);

            if (nameExists)
            {
                return Result.Failure(TheaterErrors.NameAlreadyExists);
            }

            theater.UpdateDetails(request.Name, request.Location, request.IsActive);

            _theaterRepository.Update(theater);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
