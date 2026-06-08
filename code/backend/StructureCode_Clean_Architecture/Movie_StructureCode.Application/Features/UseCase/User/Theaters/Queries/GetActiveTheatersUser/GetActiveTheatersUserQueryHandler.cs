using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.User.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Theaters.Queries.GetActiveTheatersUser
{
    internal sealed class GetActiveTheatersUserQueryHandler : IQueryHandler<GetActiveTheatersUserQuery, List<TheaterUserResponse>>
    {
        private readonly ITheaterRepository _theaterRepository;
        private const int LocalTimeZoneOffset = 7;

        public GetActiveTheatersUserQueryHandler(ITheaterRepository theaterRepository)
        {
            _theaterRepository = theaterRepository;
        }

        public async Task<Result<List<TheaterUserResponse>>> Handle(
            GetActiveTheatersUserQuery request,
            CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(LocalTimeZoneOffset));
            var filterDate = request.Date ?? today;

            var startDate = filterDate.ToDateTime(TimeOnly.MinValue).AddHours(-LocalTimeZoneOffset);
            var endDate = filterDate.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-LocalTimeZoneOffset);

            var theaters = await _theaterRepository.GetActiveTheatersWithShowingCountAsync(
                request.MovieId,
                startDate,
                endDate,
                cancellationToken);

            var response = theaters
                .Select(t => new TheaterUserResponse(
                    t.Theater.Id,
                    t.Theater.Name,
                    t.Theater.Location,
                    t.ShowingCount))
                .ToList();

            return Result.Success(response);
        }
    }
}
