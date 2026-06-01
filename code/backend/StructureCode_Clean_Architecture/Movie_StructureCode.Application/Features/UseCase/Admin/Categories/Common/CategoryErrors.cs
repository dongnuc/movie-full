using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common
{
    public static class CategoryErrors
    {
        public static Error NotFound(Guid id) =>
            new("Category.NotFound", $"Category with ID {id} was not found.");

        public static Error NameAlreadyExists =>
            new("Category.NameAlreadyExists", "A category with the same name already exists.");
    }
}
