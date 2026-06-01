using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Commands.CreateCategory
{
    public sealed record CreateCategoryCommand(
        string Name,
        bool IsActive = true
    ) : ICommand;
}
