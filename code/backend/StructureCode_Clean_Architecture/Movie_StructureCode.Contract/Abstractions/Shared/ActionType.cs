using System;

namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    [Flags]
    public enum ActionType
    {
        None = 0,
        Read = 1 << 0,       // 1
        Create = 1 << 1,     // 2
        Update = 1 << 2,     // 4
        Delete = 1 << 3,     // 8
        Import = 1 << 4,     // 16
        Export = 1 << 5,     // 32
        Approve = 1 << 6     // 64
    }
}
