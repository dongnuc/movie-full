using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.User.Showings.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Showings.Queries.GetShowingsUser
{
    internal sealed class GetShowingsUserQueryHandler : IQueryHandler<GetShowingsUserQuery, IEnumerable<ShowingUserResponse>>
    {
        private readonly IShowingRepository _showingRepository;

        public GetShowingsUserQueryHandler(IShowingRepository showingRepository)
        {
            _showingRepository = showingRepository;
        }

        public async Task<Result<IEnumerable<ShowingUserResponse>>> Handle(
            GetShowingsUserQuery request,
            CancellationToken cancellationToken)
        {
            var showings = await _showingRepository.GetActiveShowingsUserAsync(
                request.MovieId,
                request.TheaterId,
                request.Date,
                cancellationToken);

            var response = showings.Select(s => new ShowingUserResponse(
                s.Id,
                s.TimeStart,
                s.Price,
                s.RoomId));

            return Result.Success(response);
        }
    }
}
