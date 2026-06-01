using System;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesUser
{
    public sealed record GetMoviesUserQuery(
        string? Search,
        Guid? CategoryId,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<MovieUserResponse>>;
}
