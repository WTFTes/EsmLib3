using EsmLib3.Records.Abstraction;

namespace EsmLib3.Records;

public class CreatureLevList : LevelledListBase
{
    [Flags]
    public enum LevFlags
    {
        None = 0,
        AllLevels = 0x01 // Calculate from all levels <= player
        // level, not just the closest below
        // player.
    };
    
    public override RecordName Name => RecordName.LEVC;
    
    public override RecordName sRecName => RecordName.CNAM;

    public LevFlags Flags => (LevFlags)_flags;
}
