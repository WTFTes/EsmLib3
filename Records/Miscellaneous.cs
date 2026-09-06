using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Miscellaneous : AbstractRecord
{
    public enum Flags : int
    {
        None = 0,
        Key = 0x1 // Assigned as a key in the content file that defined the record
    };
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mScript { get; set; }
    
    public string mName { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }

    public MCDTstruct mData { get; set; } = new();

    public class MCDTstruct
    {
        public float mWeight { get; set; }
        public int mValue { get; set; }
        public Flags mFlags { get; set; }   // int32
    };
    
    public override RecordName Name => RecordName.MISC;

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
                case RecordName.MCDT:
                    reader.getHT(() =>
                    {
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadInt32();
                        mData.mFlags = (Flags)reader.BinaryReader.ReadInt32();
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
            throw new MissingSubrecordException(RecordName.MCDT);
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        writer.writeHNCString(RecordName.MODL, mModel);
        writer.writeHNOCString(RecordName.FNAM, mName);
        writer.writeHNT(RecordName.MCDT, () =>
        {
            writer.Write(mData.mWeight);
            writer.Write(mData.mValue);
            writer.Write((int)mData.mFlags);
        });
        writer.writeHNOCRefId(RecordName.SCRI, mScript);
        writer.writeHNOCString(RecordName.ITEX, mIcon);
    }
}