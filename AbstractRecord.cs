namespace EsmLib3;

public abstract class AbstractRecord
{
    public abstract RecordName Name { get; }
    
    public abstract void Load(EsmReader reader, out bool isDeleted);
    
    public abstract void Save(EsmWriter writer, bool isDeleted);
}
