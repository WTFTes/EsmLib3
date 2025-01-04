using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Enchantment : AbstractRecord
{
    public enum Type
    {
        CastOnce = 0,
        WhenStrikes = 1,
        WhenUsed = 2,
        ConstantEffect = 3
    }

    [Flags]
    public enum Flags
    {
        None = 0,
        Autocalc = 0x01
    }

    public class ENDTstruct
    {
        public Type mType { get; set; } // int32
        
        public int mCost { get; set; }
        
        public int mCharge { get; set; }
        
        public Flags mFlags { get; set; } // int32
    }

    public override RecordName Name => RecordName.ENCH;
    
    public RefId mId { get; set; }

    public RecordFlag mRecordFlags { get; set; }

    public ENDTstruct mData { get; set; } = new();
    
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
                case RecordName.ENDT:
                    reader.getHT(() =>
                    {
                        mData.mType = (Type)reader.BinaryReader.ReadInt32();
                        mData.mCost = reader.BinaryReader.ReadInt32();
                        mData.mCharge = reader.BinaryReader.ReadInt32();
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
            throw new MissingSubrecordException(RecordName.ENDT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}