using EsmLib3.RefIds;

namespace EsmLib3.Structs;

public class Attribute : AbstractRecord
{
    public static readonly RefId Strength = RefId.StringRefId("Strength");
    public static readonly RefId Intelligence = RefId.StringRefId("Intelligence");
    public static readonly RefId Willpower = RefId.StringRefId("Willpower");
    public static readonly RefId Agility = RefId.StringRefId("Agility");
    public static readonly RefId Speed = RefId.StringRefId("Speed");
    public static readonly RefId Endurance = RefId.StringRefId("Endurance");
    public static readonly RefId Personality = RefId.StringRefId("Personality");
    public static readonly RefId Luck = RefId.StringRefId("Luck");
    
    public static readonly RefId[] sAttributes =
    [
        Strength,
        Intelligence,
        Willpower,
        Agility,
        Speed,
        Endurance,
        Personality,
        Luck,
    ];

    public override RecordName Name => RecordName.ATTR;
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        throw new NotImplementedException();
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}