using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common
{
    public sealed record MovieResponse(
        Guid Id,
        string Title,
        string? Image,
        string CategoryName,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
