using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.DeleteMovie
{
    public sealed record DeleteMovieCommand(Guid Id) : ICommand;
}
