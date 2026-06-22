using System;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Queries.GetTheaterMembers
{
    public sealed record GetTheaterMembersQuery(
        Guid TheaterId,
        string? Search,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<TheaterMemberResponse>>;
}
