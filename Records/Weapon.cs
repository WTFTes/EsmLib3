using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Weapon : AbstractRecord
{
    public enum Type
    {
        First = -4,
        PickProbe = -4,
        HandToHand = -3,
        Spell = -2,
        None = -1,
        ShortBladeOneHand = 0,
        LongBladeOneHand = 1,
        LongBladeTwoHand = 2,
        BluntOneHand = 3,
        BluntTwoClose = 4,
        BluntTwoWide = 5,
        SpearTwoWide = 6,
        AxeOneHand = 7,
        AxeTwoHand = 8,
        MarksmanBow = 9,
        MarksmanCrossbow = 10,
        MarksmanThrown = 11,
        Arrow = 12,
        Bolt = 13,
        Last = 13
    };

    public enum AttackType
    {
        Chop = 0,
        Slash = 1,
        Thrust = 2,
    };

    [Flags]
    public enum Flags : int
    {
        None = 0,
        Magical = 0x01,
        Silver = 0x02
    };

    public class WPDTstruct
    {
        public float mWeight { get; set; }
        public int mValue { get; set; }
        public Type mType { get; set; } // int16
        public ushort mHealth { get; set; }
        public float mSpeed { get; set; }
        public float mReach { get; set; }
        public ushort mEnchant { get; set; } // Enchantment points. The real value is mEnchant/10.f
        public byte[] mChop { get; } = new byte[2]; // Min and max 
        public byte[] mSlash { get; } = new byte[2];
        public byte[] mThrust { get; } = new byte[2];
        public Flags mFlags { get; set; }   // int32
    }; // 32 bytes

    public override RecordName Name => RecordName.WEAP;

    public RefId mId { get; set; }

    public RecordFlag mRecordFlags { get; set; }

    public RefId mEnchant { get; set; }

    public RefId mScript { get; set; }

    public string mName { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }

    public WPDTstruct mData { get; set; } = new();

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
                case RecordName.WPDT:
                    reader.getHT(() =>
                    {
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadInt32();
                        mData.mType = (Type)reader.BinaryReader.ReadInt16();
                        mData.mHealth = reader.BinaryReader.ReadUInt16();
                        mData.mSpeed = reader.BinaryReader.ReadSingle();
                        mData.mReach = reader.BinaryReader.ReadSingle();
                        mData.mEnchant = reader.BinaryReader.ReadUInt16();
                        for (var i = 0; i < mData.mChop.Length; ++i)
                            mData.mChop[i] = reader.BinaryReader.ReadByte();
                        for (var i = 0; i < mData.mSlash.Length; ++i)
                            mData.mSlash[i] = reader.BinaryReader.ReadByte();
                        for (var i = 0; i < mData.mThrust.Length; ++i)
                            mData.mThrust[i] = reader.BinaryReader.ReadByte();
                        mData.mFlags = (Flags)reader.BinaryReader.ReadInt32();
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
            throw new MissingSubrecordException(RecordName.WPDT);
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
        writer.writeHNT(RecordName.WPDT, () =>
        {
            writer.Write(mData.mWeight);
            writer.Write(mData.mValue);
            writer.Write((short)mData.mType);
            writer.Write(mData.mHealth);
            writer.Write(mData.mSpeed);
            writer.Write(mData.mReach);
            writer.Write(mData.mEnchant);
            writer.Write(mData.mChop);
            writer.Write(mData.mSlash);
            writer.Write(mData.mThrust);
            writer.Write((int)mData.mFlags);
        });
        writer.writeHNOCRefId(RecordName.SCRI, mScript);
        writer.writeHNOCString(RecordName.ITEX, mIcon);
        writer.writeHNOCRefId(RecordName.ENAM, mEnchant);
    }
}
