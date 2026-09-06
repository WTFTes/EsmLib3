using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class StartScript : AbstractRecord
{
    public override RecordName Name => RecordName.SSCR;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mData { get; set; }
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        var hasData = false;
        var hasName = false;

        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mId = reader.getRefId();
                    hasName = true;
                    break;
                case RecordName.DATA:
                    mData = reader.getHString();
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
            writer.writeDeleted();
        else
            writer.writeHNString(RecordName.DATA, mData);
    }
}