using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Region : AbstractRecord
{
    public class WEATstruct
    {
        // These are probabilities that add up to 100
        // Clear, Cloudy, Foggy, Overcast, Rain, Thunder, Ash, Blight, Snow, Blizzard
        public byte[] mProbabilities { get; } = new byte[10];
    }

    public class SoundRef
    {
        public RefId mSound { get; set; }
        public byte mChance { get; set; }
    };
    
    public override RecordName Name => RecordName.REGN;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mName { get; set; }
    
    public RefId mSleepList { get; set; }

    public WEATstruct mData { get; } = new();
    
    public int mMapColor { get; set; }
    
    public List<SoundRef> mSoundList { get; set; } = new();
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();
        
        mSoundList.Clear();

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
                case RecordName.WEAT:
                    reader.getSubHeader();
                    // Cold weather not included before 1.3
                    if (reader.GetSubSize() == 10)
                    {
                        for (var i = 0; i < 10; ++i)
                            mData.mProbabilities[i] = reader.BinaryReader.ReadByte();
                    }
                    else if (reader.GetSubSize() == 8)
                    {
                        for (var i = 0; i < 8; ++i)
                            mData.mProbabilities[i] = reader.BinaryReader.ReadByte();

                        mData.mProbabilities[8] = mData.mProbabilities[9] = 0;
                    }
                    else
                        throw new Exception("Don't know what to do in this version");
                    break;
                case RecordName.BNAM:
                    mSleepList = reader.getRefId();
                    break;
                case RecordName.CNAM:
                    reader.getHT(() => mMapColor = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.SNAM:
                    reader.getSubHeader();
                    SoundRef sr = new();
                    sr.mSound = reader.getMaybeFixedRefIdSize(32);
                    sr.mChance = reader.BinaryReader.ReadByte();
                    mSoundList.Add(sr);
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


    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}