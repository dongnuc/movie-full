using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Accounts.Queries.GetUserPoints
{
    public sealed record GetUserPointsQuery(Guid UserId) : IQuery<UserPointsResponse>;
}
