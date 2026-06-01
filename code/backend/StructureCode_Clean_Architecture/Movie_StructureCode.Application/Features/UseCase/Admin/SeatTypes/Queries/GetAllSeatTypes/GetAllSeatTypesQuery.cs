using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetAllSeatTypes
{
    public sealed record GetAllSeatTypesQuery(
        Guid? Id,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<SeatTypeResponse>>;
}
