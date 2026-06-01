using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.CreateTheater
{
    internal sealed class CreateTheaterCommandHandler : ICommandHander<CreateTheaterCommand>
    {
        private readonly ITheaterRepository _theaterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTheaterCommandHandler(ITheaterRepository theaterRepository, IUnitOfWork unitOfWork)
        {
            _theaterRepository = theaterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            CreateTheaterCommand request,
            CancellationToken cancellationToken)
        {
            // Check for duplicate name
            var nameExists = await _theaterRepository.ExistsByNameAsync(request.Name, propertyName: null, excludeId: null, ct: cancellationToken);
            if (nameExists)
            {
                return Result.Failure(TheaterErrors.NameAlreadyExists);
            }

            var theater = Theater.Create(Guid.NewGuid(), request.Name, request.Location, request.IsActive);

            await _theaterRepository.AddAsync(theater, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
