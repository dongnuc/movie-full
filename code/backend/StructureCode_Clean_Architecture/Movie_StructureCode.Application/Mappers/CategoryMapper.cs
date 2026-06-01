using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Category = Movie_StructureCode.Domain.Entities.Category;

namespace Movie_StructureCode.Application.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryResponse ToResponse(Category category)
        {
            return new CategoryResponse(
                category.Id,
                category.Name,
                category.IsActive,
                category.DateCreate,
                category.DateUpdate);
        }
    }
}
