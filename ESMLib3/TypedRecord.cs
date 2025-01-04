namespace EsmLib3;

public class TypedRecord<T> : RecordBase where T : AbstractRecord, new()
{
    public T Data { get; } = new();
    public bool IsDeleted { get; set; }

    public override void Load(EsmReader reader)
    {
        Data.Load(reader, out var isDeleted);
        IsDeleted = isDeleted;
    }

    public override void Save(EsmWriter writer) => Data.Save(writer, IsDeleted);
}
