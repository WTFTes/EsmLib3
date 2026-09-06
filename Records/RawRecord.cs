namespace EsmLib3.Records;

public class RawRecord : AbstractRecord
{
    public override RecordName Name => RawName;
    
    public RecordName RawName { get; set; }
    
    private byte[] _data;

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        _data = reader.GetRawRecord();
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.Write(_data);
    }
}