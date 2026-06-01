using System.Collections.Generic;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetActiveSeatTypes
{
    public sealed record GetActiveSeatTypesQuery() : IQuery<IEnumerable<SeatTypeActiveResponse>>;
}
