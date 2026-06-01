using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Queries.GetAllMoviesAdmin
{
    public sealed record GetAllMoviesAdminQuery(
        string? Search,
        Guid? CategoryId,
        bool? IsActive,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<MovieResponse>>;
}
