using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    public record AccountResponse(
        Guid Id,
        string Username,
        string Email,
        string Role,
        bool IsActive,
        List<Guid> TheaterIds
    );
}
