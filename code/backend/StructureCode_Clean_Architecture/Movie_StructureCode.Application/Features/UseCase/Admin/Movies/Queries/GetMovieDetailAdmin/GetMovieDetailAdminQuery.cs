using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Queries.GetMovieDetailAdmin
{
    public sealed record GetMovieDetailAdminQuery(Guid Id) : IQuery<MovieDetailResponse>;
}
