using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common
{
    public static class TheaterErrors
    {
        public static Error NotFound(Guid id) => 
            new("Theater.NotFound", $"Theater with ID {id} was not found.");

        public static Error NameAlreadyExists => 
            new("Theater.NameAlreadyExists", "A theater with the same name already exists.");
    }
}
