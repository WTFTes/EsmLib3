using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Container : AbstractRecord
{
    [Flags]
    public enum Flags
    {
        None = 0,
        Organic = 1, // Objects cannot be placed in this container
        Respawn = 2, // Respawns after 4 months
        Unknown = 8
    };

    public override RecordName Name => RecordName.CONT;

    public RefId mId { get; set; }

    public RecordFlag mRecordFlags { get; set; }

    public RefId mScript { get; set; }

    public string mName { get; set; }

    public string mModel { get; set; }

    public float mWeight { get; set; }

    public Flags mFlags { get; set; } // int32

    public InventoryList mInventory { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        mInventory.mList.Clear();
        
        var hasName = false;
        var hasWeight = false;
        var hasFlags = false;
        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mId = reader.getRefId();
                    hasName = true;
                    break;
                case RecordName.MODL:
                    mModel = reader.getHString();
                    break;
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.CNDT:
                    reader.getHT(() => mWeight = reader.BinaryReader.ReadSingle());
                    hasWeight = true;
                    break;
                case RecordName.FLAG:
                    reader.getHT(() => mFlags = (Flags)reader.BinaryReader.ReadInt32());
                    if ((mFlags & (Flags)0xf4) != 0)
                        throw new Exception("Unknown flags");
                    if (!mFlags.HasFlag(Flags.Unknown))
                        throw new Exception("Flag 8 not set");
                    hasFlags = true;
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();
                    break;
                case RecordName.NPCO:
                    mInventory.Add(reader);
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (!hasName)
            throw new MissingSubrecordException(RecordName.NAME);
        if (!hasWeight && !isDeleted)
            throw new MissingSubrecordException(RecordName.CNDT);
        if (!hasFlags && !isDeleted)
            throw new MissingSubrecordException(RecordName.FLAG);
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        writer.writeHNCString(RecordName.MODL, mModel);
        writer.writeHNOCString(RecordName.FNAM, mName);
        writer.writeHNT(RecordName.CNDT, mWeight);
        writer.writeHNT(RecordName.FLAG, (int)mFlags);

        writer.writeHNOCRefId(RecordName.SCRI, mScript);

        mInventory.Save(writer);
    }
}