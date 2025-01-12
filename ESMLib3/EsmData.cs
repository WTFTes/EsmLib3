using EsmLib3.Records;

namespace EsmLib3;

public class EsmData
{
    public Header Header;

    public EsmEncoding Encoding;

    private List<RecordBase> _records = new();

    public bool HasRaw { get; private set; }

    public IEnumerable<RecordBase> Records => _records;

    public IEnumerable<T> RecordData<T>() where T : AbstractRecord, new() => Records.Where(r => r is TypedRecord<T>).Select(_ => _.GetData<T>());

    public int RecordCount => _records.Count;

    public void AddRecord(RecordBase record)
    {
        _records.Add(record);

        if (record is TypedRecord<RawRecord>)
            HasRaw = true;
    }

    public Dictionary<RecordName, List<RecordBase>> GetGroupedRecords()
    {
        return Records.Where(r => r is not TypedRecord<RawRecord>).GroupBy(r => r.mType)
            .ToDictionary(g => g.Key, _ => _.ToList());
    }

    public IEnumerable<RecordBase> GetRaw() => Records.Where(r => r is TypedRecord<RawRecord>);
}
