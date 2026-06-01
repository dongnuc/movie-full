using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.CreateTheater
{
    public sealed record CreateTheaterCommand(
        string Name,
        string? Location,
        bool IsActive = true
    ) : ICommand;
}
