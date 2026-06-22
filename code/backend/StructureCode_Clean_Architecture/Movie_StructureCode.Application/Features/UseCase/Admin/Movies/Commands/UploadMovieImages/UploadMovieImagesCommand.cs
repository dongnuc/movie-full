using System;
using System.IO;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Commands.UploadMovieImages
{
    public sealed record UploadMovieImagesCommand(
        Guid MovieId,
        Stream? BannerStream,
        string? BannerFileName,
        string? BannerContentType,
        Stream? PosterStream,
        string? PosterFileName,
        string? PosterContentType
    ) : ICommand;
}
