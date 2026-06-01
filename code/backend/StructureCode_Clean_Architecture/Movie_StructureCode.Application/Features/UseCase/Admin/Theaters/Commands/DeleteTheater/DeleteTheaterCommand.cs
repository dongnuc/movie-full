using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Commands.DeleteTheater
{
    public sealed record DeleteTheaterCommand(Guid Id) : ICommand;
}
