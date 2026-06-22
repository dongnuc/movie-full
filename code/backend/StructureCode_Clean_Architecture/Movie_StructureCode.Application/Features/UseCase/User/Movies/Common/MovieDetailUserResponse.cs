using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Common
{
    public sealed record MovieDetailUserResponse(
        Guid Id,
        string Title,
        string? Description,
        string? ImageBanner,
        string? ImagePoster,
        bool IsHomePage,
        decimal BasePrice,
        int Duration,
        Guid CategoryId,
        string CategoryName,
        IReadOnlyList<TheaterShowingGroup> Theaters);

    public sealed record TheaterShowingGroup(
        Guid TheaterId,
        string TheaterName,
        int ShowingCount);
}
