using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Armor : AbstractRecord
{
    public enum Type
    {
        Helmet = 0,
        Cuirass = 1,
        LPauldron = 2,
        RPauldron = 3,
        Greaves = 4,
        Boots = 5,
        LGauntlet = 6,
        RGauntlet = 7,
        Shield = 8,
        LBracer = 9,
        RBracer = 10
    }

    public class AODTstruct
    {
        public Type mType { get; set; }

        public float mWeight { get; set; }

        public int mValue { get; set; }

        public int mHealth { get; set; }

        public int mEnchant { get; set; }

        public int mArmor { get; set; }
    }

    public override RecordName Name => RecordName.ARMO;

    public AODTstruct mData { get; set; } = new();

    public PartReferenceList mParts { get; set; } = new();
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mScript { get; set; }
    
    public RefId mEnchant { get; set; }
    
    public string mName { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }

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
                case RecordName.AODT:
                    reader.getHT(() =>
                    {
                        mData.mType = (Type)reader.BinaryReader.ReadInt32();
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadInt32();
                        mData.mHealth = reader.BinaryReader.ReadInt32();
                        mData.mEnchant = reader.BinaryReader.ReadInt32();
                        mData.mArmor = reader.BinaryReader.ReadInt32();
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
            throw new MissingSubrecordException(RecordName.AODT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}