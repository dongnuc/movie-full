using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;
using Category = Movie_StructureCode.Domain.Entities.Category;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Commands.UpdateCategory
{
    internal sealed class UpdateCategoryCommandHandler : ICommandHander<UpdateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Failure(CategoryErrors.NotFound(request.Id));
            }

            var nameExists = await _categoryRepository.ExistsByNameAsync(request.Name, propertyName: null, excludeId: request.Id, ct: cancellationToken);
            if (nameExists)
            {
                return Result.Failure(CategoryErrors.NameAlreadyExists);
            }

            category.UpdateDetails(request.Name, request.IsActive);

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
