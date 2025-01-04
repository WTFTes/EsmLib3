using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Light : AbstractRecord
{
    [Flags]
    public enum Flags
    {
        None = 0,
        Dynamic = 0x001,
        Carry = 0x002, // Can be carried
        Negative = 0x004, // Negative light - i.e. darkness
        Flicker = 0x008,
        Fire = 0x010,
        OffDefault
            = 0x020, // Off by default - does not burn while placed in a cell, but can burn when equipped by an NPC
        FlickerSlow = 0x040,
        Pulse = 0x080,
        PulseSlow = 0x100
    };
    
    public class LHDTstruct
    {
        public float mWeight { get; set; }
        
        public int mValue { get; set; }
        
        public int mTime { get; set; } // Duration
        
        public int mRadius { get; set; }
        
        public uint mColor { get; set; } // 4-byte rgba value
        
        public Flags mFlags { get; set; } // int32
    } // Size = 24 bytes
    
    public override RecordName Name => RecordName.LIGH;
    
    public RefId mId { get; set; }
    
    public RefId mSound { get; set; }
    
    public RefId mScript { get; set; }

    public RecordFlag mRecordFlags { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }
    
    public string mName { get; set; }

    public LHDTstruct mData { get; set; } = new();

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
                case RecordName.ITEX:
                    mIcon = reader.getHString();
                    break;
                case RecordName.LHDT:
                    reader.getHT(() =>
                    {
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadInt32();
                        mData.mTime = reader.BinaryReader.ReadInt32();
                        mData.mRadius = reader.BinaryReader.ReadInt32();
                        mData.mColor = reader.BinaryReader.ReadUInt32();
                        mData.mFlags = (Flags)reader.BinaryReader.ReadInt32();
                    });
                    hasData = true;
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();
                    break;
                case RecordName.SNAM:
                    mSound = reader.getRefId();
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
            throw new MissingSubrecordException(RecordName.LHDT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}