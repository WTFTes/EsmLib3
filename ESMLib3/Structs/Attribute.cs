using EsmLib3.RefIds;

namespace EsmLib3.Structs;

public class Attribute : AbstractRecord
{
    public static readonly RefId Strength = new StringRefId("Strength");
    public static readonly RefId Intelligence = new StringRefId("Intelligence");
    public static readonly RefId Willpower = new StringRefId("Willpower");
    public static readonly RefId Agility = new StringRefId("Agility");
    public static readonly RefId Speed = new StringRefId("Speed");
    public static readonly RefId Endurance = new StringRefId("Endurance");
    public static readonly RefId Personality = new StringRefId("Personality");
    public static readonly RefId Luck = new StringRefId("Luck");
    
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