using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common
{
    public sealed record SeatTypeResponse(
        Guid Id,
        string Name,
        decimal Surcharge,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
