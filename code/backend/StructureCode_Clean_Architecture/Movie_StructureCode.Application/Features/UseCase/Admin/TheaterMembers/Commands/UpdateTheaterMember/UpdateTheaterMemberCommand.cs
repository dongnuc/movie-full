using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.UpdateTheaterMember
{
    public sealed record UpdateTheaterMemberCommand(
        Guid TheaterId,
        Guid MemberId,
        Guid RoleId
    ) : ICommand;
}
