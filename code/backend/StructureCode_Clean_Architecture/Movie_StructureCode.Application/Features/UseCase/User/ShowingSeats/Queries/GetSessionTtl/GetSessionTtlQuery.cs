using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.ShowingSeats.Queries.GetSessionTtl
{
    public sealed record GetSessionTtlResponse(Guid ShowingId, int RemainingSeconds, bool IsActive);

    public sealed record GetSessionTtlQuery(Guid ShowingId, Guid UserId) : IQuery<GetSessionTtlResponse>;
}
