using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Spell : AbstractRecord
{
    public enum SpellType : int
    {
        Spell = 0, // Normal spell, must be cast and costs mana
        Ability = 1, // Inert ability, always in effect
        Blight = 2, // Blight disease
        Disease = 3, // Common disease
        Curse = 4, // Curse (?)
        Power = 5 // Power, can use once a day
    };
    
    [Flags]
    public enum Flags
    {
        None = 0,
        Autocalc = 1, // Can be selected by NPC spells auto-calc
        PCStart = 2, // Can be selected by player spells auto-calc
        Always = 4 // Casting always succeeds
    };

    public class SPDTstruct
    {
        public SpellType mType { get; set; } // SpellType
        public int mCost { get; set; } // Mana cost
        public Flags mFlags { get; set; } // Flags
    }

    public override RecordName Name => RecordName.SPEL;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mName { get; set; }

    public SPDTstruct mData { get; set; } = new();

    public EffectList mEffects { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();
        
        mEffects.mList.Clear();

        var hasName = false;
        var hasData = false;
        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mId = reader.getRefId();
                    hasName = true;
                    break;
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.SPDT:
                    reader.getHT(() =>
                    {
                        mData.mType = (SpellType)reader.BinaryReader.ReadInt32();
                        mData.mCost = reader.BinaryReader.ReadInt32();
                        mData.mFlags = (Flags)reader.BinaryReader.ReadInt32();
                    });
                    hasData = true;
                    break;
                case RecordName.ENAM:
                    mEffects.Add(reader);
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (!hasName)
            throw new MissingSubrecordException(RecordName.NAME);

        if (!hasData && !isDeleted)
            throw new MissingSubrecordException(RecordName.SPDT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}