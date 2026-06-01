using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Queries.GetSeatTypeById
{
    public sealed record GetSeatTypeByIdQuery(Guid Id) : IQuery<SeatTypeResponse>;
}
