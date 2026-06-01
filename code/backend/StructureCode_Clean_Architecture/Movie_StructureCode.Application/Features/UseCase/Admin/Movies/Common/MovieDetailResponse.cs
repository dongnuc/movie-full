using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common
{
    public sealed record MovieDetailResponse(
        Guid Id,
        string Title,
        string? Description,
        string? Image,
        decimal BasePrice,
        int Duration,
        Guid CategoryId,
        string CategoryName,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
