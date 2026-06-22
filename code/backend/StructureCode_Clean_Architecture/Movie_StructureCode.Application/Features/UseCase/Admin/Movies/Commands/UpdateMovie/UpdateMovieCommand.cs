using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.UpdateMovie
{
    public sealed record UpdateMovieCommand(
        Guid Id,
        string Title,
        string? Description,
        string? Image,
        decimal BasePrice,
        int Duration,
        Guid CategoryId,
        bool IsActive,
        bool IsHomePage = false,
        bool ClearBanner = false,
        bool ClearPoster = false
    ) : ICommand;
}
