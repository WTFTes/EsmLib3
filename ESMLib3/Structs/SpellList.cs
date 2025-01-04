using EsmLib3.RefIds;

namespace EsmLib3.Structs;

public class SpellList
{
    public List<RefId> mList { get; set; } = new();

    /// Load one spell, assumes the subrecord name was already read
    public void Add(EsmReader reader)
    {
        mList.Add(reader.getRefId());
    }

    public void Save(EsmWriter writer)
    {
        throw new NotImplementedException();
    }
}