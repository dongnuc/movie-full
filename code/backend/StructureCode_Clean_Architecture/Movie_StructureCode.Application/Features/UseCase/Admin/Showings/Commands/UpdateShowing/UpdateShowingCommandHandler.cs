using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Commands.UpdateShowing
{
    internal sealed class UpdateShowingCommandHandler : ICommandHander<UpdateShowingCommand>
    {
        private readonly IShowingRepository _showingRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateShowingCommandHandler(
            IShowingRepository showingRepository,
            IMovieRepository movieRepository,
            IRoomRepository roomRepository,
            IUnitOfWork unitOfWork)
        {
            _showingRepository = showingRepository;
            _movieRepository = movieRepository;
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateShowingCommand request, CancellationToken cancellationToken)
        {
            var showing = await _showingRepository.GetByIdAsync(request.Id, cancellationToken);
            if (showing == null)
            {
                return Result.Failure(ShowingErrors.NotFound(request.Id));
            }

            var movie = await _movieRepository.GetByIdAsync(showing.MovieId, cancellationToken);
            if (movie == null)
            {
                return Result.Failure(ShowingErrors.MovieNotFound);
            }

            var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure(ShowingErrors.RoomNotFound);
            }

            var existingShowings = await _showingRepository.GetShowingsByRoomAsync(request.RoomId, cancellationToken);

            var newStart = request.TimeStart;
            var newEnd = newStart.AddMinutes(movie.Duration);

            var hasConflict = existingShowings.Items.Any(s =>
                s.Id != request.Id &&
                s.IsActive &&
                newStart < s.TimeStart.AddMinutes(s.Movie?.Duration ?? 0) &&
                s.TimeStart < newEnd);

            if (hasConflict)
            {
                return Result.Failure(ShowingErrors.RoomConflict);
            }

            showing.UpdateDetails(
                request.TimeStart,
                request.Price,
                request.MaxTicketsPerUser,
                request.RoomId,
                room.TheaterId,
                request.IsActive
            );

            _showingRepository.Update(showing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
