using EsmLib3.Enums;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class GameSetting : AbstractRecord
{
    public override RecordName Name => RecordName.GMST;

    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }

    public Variant mValue { get; set; } = new();
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        mId = reader.getHNRefId(RecordName.NAME);
        mValue.Read(reader, Variant.Format.Gmst);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}