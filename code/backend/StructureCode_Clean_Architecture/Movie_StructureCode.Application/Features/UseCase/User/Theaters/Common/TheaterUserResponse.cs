using System;

namespace Movie_StructureCode.Application.Features.UseCase.User.Theaters.Common
{
    public sealed record TheaterUserResponse(
        Guid Id,
        string Name,
        string? Location,
        int ShowingCount);
}
