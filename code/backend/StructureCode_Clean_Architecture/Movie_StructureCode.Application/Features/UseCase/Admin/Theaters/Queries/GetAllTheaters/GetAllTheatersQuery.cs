using Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Queries.GetAllTheaters
{
    public sealed record GetAllTheatersQuery(
        string? Search,
        bool? IsActive,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<TheaterResponse>>;
}
