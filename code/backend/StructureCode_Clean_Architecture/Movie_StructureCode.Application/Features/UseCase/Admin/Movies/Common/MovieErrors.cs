using System;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common
{
    public static class MovieErrors
    {
        public static Error NotFound(Guid id) => 
            new("Movie.NotFound", $"Movie with ID {id} was not found.");

        public static Error TitleAlreadyExists => 
            new("Movie.TitleAlreadyExists", "A movie with the same title already exists.");
    }
}
