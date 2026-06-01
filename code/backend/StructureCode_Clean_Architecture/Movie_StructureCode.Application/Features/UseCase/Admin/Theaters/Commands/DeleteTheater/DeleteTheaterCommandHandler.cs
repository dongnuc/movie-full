using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.DeleteTheater
{
    internal sealed class DeleteTheaterCommandHandler : ICommandHander<DeleteTheaterCommand>
    {
        private readonly ITheaterRepository _theaterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTheaterCommandHandler(ITheaterRepository theaterRepository, IUnitOfWork unitOfWork)
        {
            _theaterRepository = theaterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            DeleteTheaterCommand request,
            CancellationToken cancellationToken)
        {
            var theater = await _theaterRepository.GetByIdAsync(request.Id, cancellationToken);
            if (theater == null)
            {
                return Result.Failure(TheaterErrors.NotFound(request.Id));
            }

            _theaterRepository.Remove(theater);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
