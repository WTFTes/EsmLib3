using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Activator : AbstractRecord
{
    public override RecordName Name => RecordName.ACTI;

    public RefId mId { get; set; }

    public RecordFlag mRecordFlags { get; set; }

    public RefId mScript { get; set; }

    public string mName { get; set; }

    public string mModel { get; set; }

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

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
                case RecordName.MODL:
                    mModel = reader.getHString();
                    break;
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();
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
        writer.writeHNOCRefId(RecordName.SCRI, mScript);
    }
}
