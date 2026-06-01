using System;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Commands.CreateCategory
{
    internal sealed class CreateCategoryCommandHandler : ICommandHander<CreateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            CreateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var nameExists = await _categoryRepository.ExistsByNameAsync(request.Name, propertyName: null, excludeId: null, ct: cancellationToken);
            if (nameExists)
            {
                return Result.Failure(CategoryErrors.NameAlreadyExists);
            }

            var category = Category.Create(Guid.NewGuid(), request.Name, request.IsActive);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
