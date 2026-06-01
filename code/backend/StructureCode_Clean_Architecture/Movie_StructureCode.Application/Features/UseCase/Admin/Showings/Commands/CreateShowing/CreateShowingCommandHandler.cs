using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Commands.CreateShowing
{
    internal sealed class CreateShowingCommandHandler : ICommandHander<CreateShowingCommand>
    {
        private readonly IShowingRepository _showingRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateShowingCommandHandler(
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

        public async Task<Result> Handle(CreateShowingCommand request, CancellationToken cancellationToken)
        {
            var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);
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
                s.IsActive &&
                newStart < s.TimeStart.AddMinutes(s.Movie?.Duration ?? 0) &&
                s.TimeStart < newEnd);

            if (hasConflict)
            {
                return Result.Failure(ShowingErrors.RoomConflict);
            }

            var showing = Showing.Create(
                Guid.NewGuid(),
                request.TimeStart,
                request.Price,
                request.MaxTicketsPerUser,
                request.MovieId,
                request.RoomId,
                room.TheaterId,
                request.IsActive
            );

            await _showingRepository.AddAsync(showing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
