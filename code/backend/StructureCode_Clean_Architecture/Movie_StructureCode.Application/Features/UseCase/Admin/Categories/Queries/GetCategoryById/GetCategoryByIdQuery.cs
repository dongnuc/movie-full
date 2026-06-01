using System;
using Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Queries.GetCategoryById
{
    public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryResponse>;
}
