using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

/*
 * Texture used for texturing landscape.
 * They are indexed by 'num', but still use 'id' to override base records.
 * Original editor even does not allow to create new records with existing ID's.
 */
public class LandTexture : AbstractRecord
{
    public override RecordName Name => RecordName.LTEX;
    
    public RefId mId { get; set; }
    
    public string mTexture { get; set; }
    
    public uint mIndex { get; set; }

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;

        var hasName = false;
        var hasIndex = false;
        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mId = reader.getRefId();
                    hasName = true;
                    break;
                case RecordName.INTV:
                    reader.getHT(() => mIndex = reader.BinaryReader.ReadUInt32());
                    hasIndex = true;
                    break;
                case RecordName.DATA:
                    mTexture = reader.getHString();
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (!hasName)
            throw new MissingSubrecordException(RecordName.NAME);
        if (!hasIndex)
            throw new MissingSubrecordException(RecordName.INTV);
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);
        writer.writeHNT(RecordName.INTV, mIndex);
        writer.writeHNCString(RecordName.DATA, mTexture);

        if (isDeleted)
            writer.writeDeleted();
    }
}