namespace EsmLib3.Enums;

[Flags]
public enum RecordFlag
{
    None = 0,
    // This flag exists, but is not used to determine if a record has been deleted while loading
    FLAG_Deleted = 0x00000020,
    FLAG_Persistent = 0x00000400,
    FLAG_Ignored = 0x00001000,
    FLAG_Blocked = 0x00002000
}
