using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Queries.GetAllCategories
{
    public sealed record GetAllCategoriesQuery(
        Guid? Id,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<PagedResult<CategoryResponse>>;
}
