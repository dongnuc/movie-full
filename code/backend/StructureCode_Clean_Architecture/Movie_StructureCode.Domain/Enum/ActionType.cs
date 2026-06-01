namespace Movie_StructureCode.Domain.Enum;

public enum ActionType
{
    Deny = 0,

    View = 1,
    Create = 2,
    Update = 4,
    Delete = 8,



    Approve = 16,
    Execute = 32,

    Export = 64,
    Import = 128,

    Revoke = 256,

    FullControl = 511

}
