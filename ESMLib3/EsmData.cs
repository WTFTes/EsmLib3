namespace EsmLib3;

public class EsmData
{
    public Header Header;
    public RecordName Format { get; set; }
    public List<RecordBase> Records { get; set; } = new();
}