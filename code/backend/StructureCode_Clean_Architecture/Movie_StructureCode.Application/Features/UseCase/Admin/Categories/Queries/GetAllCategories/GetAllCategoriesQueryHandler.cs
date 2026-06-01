using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Queries.GetAllCategories
{
    internal sealed class GetAllCategoriesQueryHandler : IQueryHandler<GetAllCategoriesQuery, PagedResult<CategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<PagedResult<CategoryResponse>>> Handle(
            GetAllCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(
                request.Id,
                request.Page,
                request.PageSize,
                cancellationToken);

            var responseItems = categories.Items.Select(CategoryMapper.ToResponse).ToList();
            var result = PagedResult<CategoryResponse>.Create(
                responseItems,
                categories.PageIndex,
                categories.PageSize,
                categories.TotalCount);

            return Result.Success(result);
        }
    }
}
