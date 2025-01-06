using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class BirthSign : AbstractRecord
{
    public override RecordName Name => RecordName.BSGN;

    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mId { get; set; }
    
    public string mName { get; set; }
    
    public string mDescription { get; set; }
    
    public string mTexture { get; set; }

    public SpellList mPowers { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        mPowers.mList.Clear();

        var hasName = false;
        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mId = reader.getRefId();
                    hasName = true;
                    break;
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.TNAM:
                    mTexture = reader.getHString();
                    break;
                case RecordName.DESC:
                    mDescription = reader.getHString();
                    break;
                case RecordName.NPCS:
                    mPowers.Add(reader);
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
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }
        
        writer.writeHNOCString(RecordName.FNAM, mName);
        writer.writeHNOCString(RecordName.TNAM, mTexture);
        writer.writeHNOCString(RecordName.DESC, mDescription);

        mPowers.Save(writer);
    }
}