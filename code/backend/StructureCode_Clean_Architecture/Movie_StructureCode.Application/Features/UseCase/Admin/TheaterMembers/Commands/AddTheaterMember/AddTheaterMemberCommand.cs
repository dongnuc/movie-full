using System;
using Movie_StructureCode.Contract.Abstractions.Message;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.TheaterMembers.Commands.AddTheaterMember
{
    public sealed record AddTheaterMemberCommand(
        Guid TheaterId,
        string Username,
        Guid RoleId
    ) : ICommand;
}
