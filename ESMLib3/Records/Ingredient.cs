using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Ingredient : AbstractRecord
{
    public class IRDTstruct
    {
        public float mWeight { get; set; }

        public int mValue { get; set; }

        public int[] mEffectID { get; } = new int[4]; // Effect, -1 means none

        public int[] mSkills { get; } = new int[4]; // SkillEnum related to effect

        public int[] mAttributes { get; } = new int[4]; // Attribute related to effect
    }

    public override RecordName Name => RecordName.INGR;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mScript { get; set; }
    
    public string mName { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }

    public IRDTstruct mData { get; set; } = new();
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

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
                case RecordName.MODL:
                    mModel = reader.getHString();
                    break;
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.IRDT:
                    reader.getHT(() =>
                    {
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadInt32();
                        for (var i = 0; i < mData.mEffectID.Length; ++i)
                            mData.mEffectID[i] = reader.BinaryReader.ReadInt32();
                        for (var i = 0; i < mData.mSkills.Length; ++i)
                            mData.mSkills[i] = reader.BinaryReader.ReadInt32();
                        for (var i = 0; i < mData.mAttributes.Length; ++i)
                            mData.mAttributes[i] = reader.BinaryReader.ReadInt32();
                    });
                    hasData = true;
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();
                    break;
                case RecordName.ITEX:
                    mIcon = reader.getHString();
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
            throw new MissingSubrecordException(RecordName.IRDT);
        
        // horrible hack to fix broken data in records
        for (var i = 0; i < 4; ++i)
        {
            if (mData.mEffectID[i] != 85 && mData.mEffectID[i] != 22 && mData.mEffectID[i] != 17
                && mData.mEffectID[i] != 79 && mData.mEffectID[i] != 74)
            {
                mData.mAttributes[i] = -1;
            }

            // is this relevant in cycle from 0 to 4?
            if (mData.mEffectID[i] != 89 && mData.mEffectID[i] != 26 && mData.mEffectID[i] != 21
                && mData.mEffectID[i] != 83 && mData.mEffectID[i] != 78)
            {
                mData.mSkills[i] = -1;
            }
        }
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}