using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Commands.DeleteCategory
{
    public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
}
