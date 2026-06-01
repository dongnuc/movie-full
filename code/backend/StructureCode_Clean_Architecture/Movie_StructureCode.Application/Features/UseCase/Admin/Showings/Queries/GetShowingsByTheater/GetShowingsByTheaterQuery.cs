using System;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingsByTheater
{
    public sealed record GetShowingsByTheaterQuery(
        Guid TheaterId,
        DateOnly? DateStart,
        int PageIndex,
        int PageSize,
        bool? IsActive
    ) : IQuery<PagedResult<ShowingGroupedByMovie>>;
}
