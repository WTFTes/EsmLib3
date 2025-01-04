using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Body : AbstractRecord
{
    public enum MeshPart : byte
    {
        Head = 0,
        Hair = 1,
        Neck = 2,
        Chest = 3,
        Groin = 4,
        Hand = 5,
        Wrist = 6,
        Forearm = 7,
        Upperarm = 8,
        Foot = 9,
        Ankle = 10,
        Knee = 11,
        Upperleg = 12,
        Clavicle = 13,
        Tail = 14,
    };

    [Flags]
    public enum Flags : byte
    {
        None = 0,
        Female = 1,
        NotPlayable = 2
    };

    public enum MeshType : byte
    {
        Skin = 0,
        Clothing = 1,
        Armor = 2
    };

    public class BYDTstruct
    {
        public MeshPart mPart { get; set; } // mesh part
        public byte mVampire { get; set; } // boolean
        public Flags mFlags { get; set; }
        public MeshType mType { get; set; } // mesh type
    }

    public override RecordName Name => RecordName.BODY;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mRace { get; set; }
    
    public string mModel { get; set; }

    public BYDTstruct mData { get; set; } = new();
    
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
                    mRace = reader.getRefId();
                    break;
                case RecordName.BYDT:
                    reader.getHT(() =>
                    {
                        mData.mPart = (MeshPart)reader.BinaryReader.ReadByte();
                        mData.mVampire = reader.BinaryReader.ReadByte();
                        mData.mFlags = (Flags)reader.BinaryReader.ReadByte();
                        mData.mType = (MeshType)reader.BinaryReader.ReadByte();
                    });
                    hasData = true;
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
            throw new MissingSubrecordException(RecordName.BYDT);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}