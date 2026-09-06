using EsmLib3.Enums;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Global : AbstractRecord
{
    public override RecordName Name => RecordName.GLOB;

    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public Variant mValue { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        mId = reader.getHNRefId(RecordName.NAME);

        if (reader.IsNextSub(RecordName.DELE))
        {
            reader.skipHSub();
            isDeleted = true;
        }
        else
            mValue.Read(reader, Variant.Format.Global);
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
            writer.writeDeleted();
        else
            mValue.Write(writer, Variant.Format.Global);
    }
}