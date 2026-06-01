using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Seats.Commands.UpdateSeatsTypeInBulk
{
    public sealed record UpdateSeatsTypeInBulkCommand(
        List<Guid> SeatIds,
        Guid SeatTypeId
    ) : ICommand;
}
