using System;

namespace Movie_StructureCode.Application.Features.UseCase.User.Categories.Common
{
    public sealed record CategoryUserResponse(
        Guid Id,
        string Name);
}
