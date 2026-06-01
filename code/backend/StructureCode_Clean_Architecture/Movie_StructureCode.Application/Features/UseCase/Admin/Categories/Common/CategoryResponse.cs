using System;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common
{
    public sealed record CategoryResponse(
        Guid Id,
        string Name,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
