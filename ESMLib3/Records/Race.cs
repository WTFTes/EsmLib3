using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Race : AbstractRecord
{
    public struct SkillBonus
    {
        public int mSkill; // SkillEnum
        public int mBonus;
    };

    [Flags]
    public enum Flags : int
    {
        None = 0,
        Playable = 1,
        Beast = 2,
    }
    
    public class RADTstruct
    {
        // List of skills that get a bonus
        public SkillBonus[] mBonus { get; } = new SkillBonus[7];

        // Attribute values for male/female
        public int[] mAttributeValues { get; } = new int[16];

        // The actual eye level height (in game units) is (probably) given
        // as 'height' times 128. This has not been tested yet.
        public float mMaleHeight { get; set; }
        
        public float mFemaleHeight { get; set; }
        
        public float mMaleWeight { get; set; }
        
        public float mFemaleWeight { get; set; }

        public Flags mFlags { get; set; } // 0x1 - playable, 0x2 - beast race

        public void Load(EsmReader reader)
        {
            reader.getSubHeader();
            for (var i = 0; i < mBonus.Length; ++i)
            {
                var bonus = new SkillBonus();
                bonus.mSkill = reader.BinaryReader.ReadInt32();
                bonus.mBonus = reader.BinaryReader.ReadInt32();
                mBonus[i] = bonus;
            }

            for (var i = 0; i < mAttributeValues.Length; ++i)
                mAttributeValues[i] = reader.BinaryReader.ReadInt32();
            
            mMaleHeight = reader.BinaryReader.ReadSingle();
            mFemaleHeight = reader.BinaryReader.ReadSingle();
            mMaleWeight = reader.BinaryReader.ReadSingle();
            mFemaleWeight = reader.BinaryReader.ReadSingle();
            mFlags = (Flags)reader.BinaryReader.ReadInt32();
        }

        public void Save(EsmWriter writer)
        {
            throw new Exception("The method is not implemented."); 
        }

    }; // Size = 140 bytes
    
    public override RecordName Name => RecordName.RACE;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mName { get; set; }
    
    public string mDescription { get; set; }

    public RADTstruct mData { get; set; } = new();

    public SpellList mPowers { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        var hasName = false;
        var hasData = false;

        mData = new();

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
                case RecordName.RADT:
                    mData.Load(reader);
                    hasData = true;
                    break;
                case RecordName.DESC:
                    mDescription = reader.getHString();
                    break;
                case RecordName.NPCS:
                    mPowers.Add(reader);
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
            throw new MissingSubrecordException(RecordName.RADT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}
