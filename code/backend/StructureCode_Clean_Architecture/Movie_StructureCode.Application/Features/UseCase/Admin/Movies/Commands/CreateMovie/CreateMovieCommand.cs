using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.CreateMovie
{
    public sealed record CreateMovieCommand(
        string Title,
        string? Description,
        string? Image,
        decimal BasePrice,
        int Duration,
        Guid CategoryId,
        bool IsActive = true
    ) : ICommand;
}
