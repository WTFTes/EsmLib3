using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Skill : AbstractRecord
{
    public static readonly RefId Block = RefId.StringRefId("Block");
    public static readonly RefId Armorer = RefId.StringRefId("Armorer");
    public static readonly RefId MediumArmor = RefId.StringRefId("MediumArmor");
    public static readonly RefId HeavyArmor = RefId.StringRefId("HeavyArmor");
    public static readonly RefId BluntWeapon = RefId.StringRefId("BluntWeapon");
    public static readonly RefId LongBlade = RefId.StringRefId("LongBlade");
    public static readonly RefId Axe = RefId.StringRefId("Axe");
    public static readonly RefId Spear = RefId.StringRefId("Spear");
    public static readonly RefId Athletics = RefId.StringRefId("Athletics");
    public static readonly RefId Enchant = RefId.StringRefId("Enchant");
    public static readonly RefId Destruction = RefId.StringRefId("Destruction");
    public static readonly RefId Alteration = RefId.StringRefId("Alteration");
    public static readonly RefId Illusion = RefId.StringRefId("Illusion");
    public static readonly RefId Conjuration = RefId.StringRefId("Conjuration");
    public static readonly RefId Mysticism = RefId.StringRefId("Mysticism");
    public static readonly RefId Restoration = RefId.StringRefId("Restoration");
    public static readonly RefId Alchemy = RefId.StringRefId("Alchemy");
    public static readonly RefId Unarmored = RefId.StringRefId("Unarmored");
    public static readonly RefId Security = RefId.StringRefId("Security");
    public static readonly RefId Sneak = RefId.StringRefId("Sneak");
    public static readonly RefId Acrobatics = RefId.StringRefId("Acrobatics");
    public static readonly RefId LightArmor = RefId.StringRefId("LightArmor");
    public static readonly RefId ShortBlade = RefId.StringRefId("ShortBlade");
    public static readonly RefId Marksman = RefId.StringRefId("Marksman");
    public static readonly RefId Mercantile = RefId.StringRefId("Mercantile");
    public static readonly RefId Speechcraft = RefId.StringRefId("Speechcraft");
    public static readonly RefId HandToHand = RefId.StringRefId("HandToHand");

    public static readonly RefId[] sSkills =
    [
        Block,
        Armorer,
        MediumArmor,
        HeavyArmor,
        BluntWeapon,
        LongBlade,
        Axe,
        Spear,
        Athletics,
        Enchant,
        Destruction,
        Alteration,
        Illusion,
        Conjuration,
        Mysticism,
        Restoration,
        Alchemy,
        Unarmored,
        Security,
        Sneak,
        Acrobatics,
        LightArmor,
        ShortBlade,
        Marksman,
        Mercantile,
        Speechcraft,
        HandToHand,
    ];

    public class SKDTstruct
    {
        public int mAttribute { get; set; } // see defs.hpp
        
        public int mSpecialization { get; set; } // 0 - Combat, 1 - Magic, 2 - Stealth

        public float[] mUseValue { get; } = new float[4]; // How much skill improves through use. Meaning
        // of each field depends on what skill this
        // is. See UseType above
    } // Total size: 24 bytes
    
    public override RecordName Name => RecordName.SKIL;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }

    public SKDTstruct mData { get; set; } = new();
    
    public string mDescription { get; set; }

    private static RefId indexToRefId(int index)
    {
        if (index < 0 || index >= sSkills.Length)
            return new RefId();

        return sSkills[index];
    }

    private static int refIdToIndex(RefId id)
    {
        for (var i = 0; i < sSkills.Length; ++i)
        {
            if (sSkills[i].Equals(id))
                return i;
        }

        return -1;
    }

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false; // Skill record can't be deleted now (may be changed in the future)
        mRecordFlags = reader.getRecordFlags();

        var hasIndex = false;
        var hasData = false;
        int index = -1;
        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.INDX:
                    reader.getHT(() => index = reader.BinaryReader.ReadInt32());
                    hasIndex = true;
                    break;
                case RecordName.SKDT:
                    reader.getHT(() =>
                    {
                        mData.mAttribute = reader.BinaryReader.ReadInt32();
                        mData.mSpecialization = reader.BinaryReader.ReadInt32();
                        for (var i = 0; i < mData.mUseValue.Length; ++i)
                            mData.mUseValue[i] = reader.BinaryReader.ReadSingle();
                    });
                    hasData = true;
                    break;
                case RecordName.DESC:
                    mDescription=reader.getHString();
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }
        
        if (!hasIndex)
            throw new MissingSubrecordException(RecordName.INDX);
        else if (index < 0 || index >= sSkills.Length)
            throw new Exception("Invalid INDX");
        if (!hasData)
            throw new MissingSubrecordException(RecordName.SKDT);

        mId = indexToRefId(index);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}