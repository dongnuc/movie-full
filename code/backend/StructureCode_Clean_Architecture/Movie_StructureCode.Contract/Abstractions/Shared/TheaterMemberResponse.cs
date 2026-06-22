using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    public record TheaterMemberResponse(
        Guid Id,
        string Username,
        string Email,
        List<string> Roles,
        bool IsActive
    );
}
