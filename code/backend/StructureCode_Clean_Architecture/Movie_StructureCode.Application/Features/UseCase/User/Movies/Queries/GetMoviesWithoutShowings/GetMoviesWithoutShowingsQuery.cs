using System.Collections.Generic;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMoviesWithoutShowings
{
    public sealed record GetMoviesWithoutShowingsQuery() : IQuery<List<MovieUserResponse>>;
}
