using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.User.Categories.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.User.Categories.Queries.GetActiveCategoriesUser
{
    internal sealed class GetActiveCategoriesUserQueryHandler : IQueryHandler<GetActiveCategoriesUserQuery, List<CategoryUserResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetActiveCategoriesUserQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<List<CategoryUserResponse>>> Handle(
            GetActiveCategoriesUserQuery request,
            CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllActiveAsync(cancellationToken);

            var response = categories.Select(c => new CategoryUserResponse(
                c.Id,
                c.Name
            )).ToList();

            return Result.Success(response);
        }
    }
}
