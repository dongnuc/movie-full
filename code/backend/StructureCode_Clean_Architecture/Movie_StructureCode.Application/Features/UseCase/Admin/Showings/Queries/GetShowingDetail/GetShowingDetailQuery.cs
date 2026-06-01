using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Queries.GetShowingDetail
{
    public sealed record GetShowingDetailQuery(Guid Id) : IQuery<ShowingDetailResponse>;
}
