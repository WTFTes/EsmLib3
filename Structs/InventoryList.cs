using EsmLib3.RefIds;

namespace EsmLib3.Structs;

/// InventoryList, NPCO subrecord
public class InventoryList
{
    public class ContItem
    {
        public int mCount { get; set; }
        public RefId mItem { get; set; }
    }

    public List<ContItem> mList { get; set; } = new();

    /// Load one item, assumes subrecord name is already read
    public void Add(EsmReader reader)
    {
        reader.getSubHeader();
        ContItem ci = new();
        ci.mCount = reader.BinaryReader.ReadInt32();
        ci.mItem = reader.getMaybeFixedRefIdSize(32);
        mList.Add(ci);
    }

    public void Save(EsmWriter writer)
    {
        foreach (var ci in mList)
        {
            writer.startSubRecord(RecordName.NPCO);
            writer.Write(ci.mCount);
            writer.writeMaybeFixedSizeRefId(ci.mItem, 32);
            writer.endRecord(RecordName.NPCO);
        }
    }
}
