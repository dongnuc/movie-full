using System;

namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    public record RoleResponse(
        Guid Id,
        string Name
    );
}
