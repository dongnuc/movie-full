namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common
{
    public sealed record TheaterResponse(
        Guid Id,
        string Name,
        string? Location,
        bool IsActive,
        DateTime DateCreate,
        DateTime DateUpdate);
}
