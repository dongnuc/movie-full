using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.RemoveTheaterMember
{
    public sealed record RemoveTheaterMemberCommand(
        Guid TheaterId,
        Guid MemberId
    ) : ICommand;
}
