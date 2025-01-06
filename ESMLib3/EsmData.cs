namespace EsmLib3;

public class EsmData
{
    public Header Header;
    public RecordName Format { get; set; }
    public List<RecordBase> Records { get; set; } = new();

    public Dictionary<RecordName, List<RecordBase>> GetGroupedRecords()
    {
        return Records.GroupBy(_ => _.mType).ToDictionary(_ => _.Key, _ => _.ToList());
    }
}