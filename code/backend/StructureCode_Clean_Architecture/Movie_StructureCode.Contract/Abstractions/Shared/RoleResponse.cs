using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    public record RoleResponse(
        Guid Id,
        string Name,
        Dictionary<string, int> Permissions
    );
}
