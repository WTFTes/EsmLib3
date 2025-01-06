using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Sound : AbstractRecord
{
    public class SOUNstruct
    {
        public byte mVolume { get; set; }
        
        public byte mMinRange { get; set; }
        
        public byte mMaxRange { get; set; }
    }

    public override RecordName Name => RecordName.SOUN;

    public RefId mId { get; set; }
    public RecordFlag mRecordFlags { get; set; }
    public string mSound { get; set; }

    public SOUNstruct mData { get; set; } = new();

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
                case RecordName.FNAM:
                    mSound = reader.getHString();
                    break;
                case RecordName.DATA:
                    reader.getHT(() =>
                    {
                        mData.mVolume = reader.BinaryReader.ReadByte();
                        mData.mMinRange = reader.BinaryReader.ReadByte();
                        mData.mMaxRange = reader.BinaryReader.ReadByte();
                    });
                    hasData = true;
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
            throw new MissingSubrecordException(RecordName.DATA);
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }
        
        writer.writeHNOCString(RecordName.FNAM, mSound);
        writer.writeHNT(RecordName.DATA, () =>
        {
            writer.Write(mData.mVolume);
            writer.Write(mData.mMinRange);
            writer.Write(mData.mMaxRange);
        });
    }
}