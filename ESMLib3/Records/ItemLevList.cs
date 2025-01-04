using EsmLib3.Records.Abstraction;

namespace EsmLib3.Records;

public class ItemLevList : LevelledListBase
{
    [Flags]
    public enum LevFlags
    {
        None = 0,
        Each = 0x01, // Select a new item each time this
        // list is instantiated, instead of
        // giving several identical items
        // (used when a container has more
        // than one instance of one levelled
        // list.)
        AllLevels = 0x02 // Calculate from all levels <= player
        // level, not just the closest below
        // player.
    };
    
    public override RecordName Name => RecordName.LEVI;
    
    public override RecordName sRecName => RecordName.INAM;
    
    public LevFlags Flags => (LevFlags)_flags;
}
