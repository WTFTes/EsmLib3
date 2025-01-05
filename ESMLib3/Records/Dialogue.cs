using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Dialogue : AbstractRecord
{
    public enum Type : sbyte
    {
        Topic = 0,
        Voice = 1,
        Greeting = 2,
        Persuasion = 3,
        Journal = 4,
        Unknown = -1 // Used for deleted dialogues
    }

    public override RecordName Name => RecordName.DIAL;
    
    public RefId mId { get; set; }
    
    public string mStringId { get; set; }

    public Type mType { get; set; } // sbyte

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        var localDeleted = false;
        if (reader.getFormatVersion() <= FormatVersion.MaxStringRefIdFormatVersion)
        {
            mStringId = reader.getHNString(RecordName.NAME);
            mId = RefId.StringRefId(mStringId);
        }
        else if (reader.getFormatVersion() <= FormatVersion.MaxNameIsRefIdOnlyFormatVersion)
            mId = reader.getHNRefId(RecordName.NAME);
        else
            mId = reader.getHNRefId(RecordName.ID__);

        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mStringId = reader.getHNString(RecordName.NAME);
                    break;
                case RecordName.DATA:
                    reader.getSubHeader();
                    var size = reader.GetSubSize();
                    if (size == 1)
                        mType = (Type)reader.BinaryReader.ReadByte();
                    else
                    {
                        reader.skip(size);
                        mType = Type.Unknown;
                    }
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    localDeleted = true;
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        isDeleted = localDeleted;
        
        if (!localDeleted && FormatVersion.MaxStringRefIdFormatVersion < reader.getFormatVersion()
                          && reader.getFormatVersion() <= FormatVersion.MaxNameIsRefIdOnlyFormatVersion)
            mStringId = mId.ToString();
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}