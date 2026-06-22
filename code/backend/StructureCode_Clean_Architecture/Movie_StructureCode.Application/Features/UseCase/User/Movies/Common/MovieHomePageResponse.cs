using System;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Common
{
    public sealed record MovieHomePageResponse(
        Guid Id,
        string Title,
        string? Description,
        string? ImageBanner,
        string? ImagePoster,
        int Duration,
        decimal BasePrice,
        string CategoryName);
}
