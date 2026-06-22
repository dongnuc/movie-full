using System;

namespace Movie_StructureCode.Application.Features.UseCase.User.Movies.Common
{
    public sealed record MovieUserResponse(
        Guid Id,
        string Title,
        string? ImagePoster,
        bool IsHomePage,
        int Duration,
        decimal BasePrice,
        Guid CategoryId,
        string CategoryName);
}
