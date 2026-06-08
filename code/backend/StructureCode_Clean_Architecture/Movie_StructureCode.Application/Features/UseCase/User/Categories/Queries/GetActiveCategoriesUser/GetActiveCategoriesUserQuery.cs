using System.Collections.Generic;
using Movie_StructureCode.Application.Features.UseCase.User.Categories.Common;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.User.Categories.Queries.GetActiveCategoriesUser
{
    public sealed record GetActiveCategoriesUserQuery : IQuery<List<CategoryUserResponse>>;
}
