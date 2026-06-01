using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common
{
    public sealed record SeatTypeActiveResponse(
        Guid Id,
        string Name,
        decimal Surcharge);
}
