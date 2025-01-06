using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Clothing : AbstractRecord
{
    public enum Type
    {
        Pants = 0,
        Shoes = 1,
        Shirt = 2,
        Belt = 3,
        Robe = 4,
        RGlove = 5,
        LGlove = 6,
        Skirt = 7,
        Ring = 8,
        Amulet = 9
    }

    public class CTDTstruct
    {
        public Type mType { get; set; } // int32
        
        public float mWeight { get; set; }
        
        public ushort mValue { get; set; }
        
        public ushort mEnchant { get; set; }
    }

    public override RecordName Name => RecordName.CLOT;

    public RefId mId { get; set; }
    
    public RefId mEnchant { get; set; }
    
    public RefId mScript { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }
    
    public string mName { get; set; }
    
    public CTDTstruct mData { get; set; } = new();

    public PartReferenceList mParts { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        mParts.mParts.Clear();

        var hasName = false;
        var hasData = false;
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
                case RecordName.CTDT:
                    reader.getHT(() =>
                    {
                        mData.mType = (Type)reader.BinaryReader.ReadInt32();
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadUInt16();
                        mData.mEnchant = reader.BinaryReader.ReadUInt16();
                    });
                    hasData = true;
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();
                    break;
                case RecordName.ITEX:
                    mIcon = reader.getHString();
                    break;
                case RecordName.ENAM:
                    mEnchant = reader.getRefId();
                    break;
                case RecordName.INDX:
                    mParts.Add(reader);
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
        if (!hasData && !isDeleted)
            throw new MissingSubrecordException(RecordName.CTDT);
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
        writer.writeHNT(RecordName.CTDT, () =>
        {
            writer.Write((int)mData.mType);
            writer.Write(mData.mWeight);
            writer.Write(mData.mValue);
            writer.Write(mData.mEnchant);
        });

        writer.writeHNOCRefId(RecordName.SCRI, mScript);
        writer.writeHNOCString(RecordName.ITEX, mIcon);

        mParts.Save(writer);

        writer.writeHNOCRefId(RecordName.ENAM, mEnchant);
    }
}
