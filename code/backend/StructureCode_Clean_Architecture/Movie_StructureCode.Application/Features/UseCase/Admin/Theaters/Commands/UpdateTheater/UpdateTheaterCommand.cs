using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.UpdateTheater
{
    public sealed record UpdateTheaterCommand(
        Guid Id,
        string Name,
        string? Location,
        bool IsActive
    ) : ICommand;
}
