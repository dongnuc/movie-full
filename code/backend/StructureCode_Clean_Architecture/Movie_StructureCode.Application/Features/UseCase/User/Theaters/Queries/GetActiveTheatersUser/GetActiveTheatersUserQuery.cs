using System;
using System.Collections.Generic;
using Movie_StructureCode.Application.Features.UseCase.User.Theaters.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Theaters.Queries.GetActiveTheatersUser
{
    public sealed record GetActiveTheatersUserQuery(Guid MovieId, DateOnly? Date) : IQuery<List<TheaterUserResponse>>;
}
