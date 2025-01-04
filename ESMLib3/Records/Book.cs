using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Book : AbstractRecord
{
    public class BKDTstruct
    {
        public float mWeight { get; set; }
        
        public int mValue { get; set; }
        
        public int mIsScroll { get; set; }
        
        public int mSkillId { get; set; }
        
        public int mEnchant { get; set; }
    }

    public override RecordName Name => RecordName.BOOK;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mScript { get; set; }
    
    public RefId mEnchant { get; set; }
    
    public string mName { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }
    
    public string mText { get; set; }

    public BKDTstruct mData { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

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
                case RecordName.BKDT:
                    reader.getHT(() =>
                    {
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadInt32();
                        mData.mIsScroll = reader.BinaryReader.ReadInt32();
                        mData.mSkillId = reader.BinaryReader.ReadInt32();
                        mData.mEnchant = reader.BinaryReader.ReadInt32();
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
                case RecordName.TEXT:
                    mText = reader.getHString();
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
            throw new MissingSubrecordException(RecordName.BKDT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}