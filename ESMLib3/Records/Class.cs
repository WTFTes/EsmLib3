using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Class : AbstractRecord
{
    public class CLDTstruct
    {
        public readonly int[] mAttribute = new int[2]; // Attributes that get class bonus

        public int mSpecialization { get; set; } // 0 = Combat, 1 = Magic, 2 = Stealth

        public int[,] mSkills { get; } = new int[5, 2]; // int[5][2] Minor and major skills.

        public int mIsPlayable { get; set; } // 0x0001 - Playable class

        public int mServices { get; set; }
    }

    public override RecordName Name => RecordName.CLAS;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mName { get; set; }
    
    public string mDescription { get; set; }

    public CLDTstruct mData { get; set; } = new();

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
                {
                    mId = reader.getRefId();
                    hasName = true;
                    break;
                }
                case RecordName.FNAM:
                {
                    mName = reader.getHString();
                    break;
                }
                case RecordName.CLDT:
                {
                    reader.getHT(() =>
                    {
                        for (var i = 0; i < mData.mAttribute.Length; ++i)
                            mData.mAttribute[i] = reader.BinaryReader.ReadInt32();
                        
                        mData.mSpecialization = reader.BinaryReader.ReadInt32();

                        for (var i = 0; i < mData.mSkills.GetLength(0); ++i)
                            for (var j = 0; j < mData.mSkills.GetLength(1); ++j)
                                mData.mSkills[i, j] = reader.BinaryReader.ReadInt32();

                        mData.mIsPlayable = reader.BinaryReader.ReadInt32();
                        mData.mServices = reader.BinaryReader.ReadInt32();
                        if (mData.mIsPlayable > 1)
                            throw new Exception("Unknown bool value");
                    });
                    hasData = true;
                    break;
                }
                case RecordName.DESC:
                {
                    mDescription = reader.getHString();
                    break;
                }
                case RecordName.DELE:
                {
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                }
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (!hasName)
            throw new MissingSubrecordException(RecordName.NAME);
        if (!hasData && !isDeleted)
            throw new MissingSubrecordException(RecordName.CLDT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}