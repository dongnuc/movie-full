using System;
using System.Collections.Generic;
using Movie_StructureCode.Application.Features.UseCase.User.Showings.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Showings.Queries.GetShowingsUser
{
    public sealed record GetShowingsUserQuery(
        Guid MovieId,
        Guid TheaterId,
        DateOnly Date
    ) : IQuery<IEnumerable<ShowingUserResponse>>;
}
