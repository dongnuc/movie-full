using System;
using Movie_StructureCode.Application.Features.UseCase.User.Movies.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Queries.GetMovieDetailUser
{
    public sealed record GetMovieDetailUserQuery(Guid MovieId) : IQuery<MovieDetailUserResponse>;
}
