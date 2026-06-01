using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.CreateSeatsInBulk
{
    public sealed record CreateSeatsInBulkCommand(Guid RoomId) : ICommand;
}
