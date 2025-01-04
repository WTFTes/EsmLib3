using System.Diagnostics;
using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;
using Attribute = EsmLib3.Structs.Attribute;

namespace EsmLib3.Records;

public class Creature : AbstractRecord
{
    // Default is 0x48?
    [Flags]
    public enum Flags
    {
        None = 0,
        Bipedal = 0x01,
        Respawn = 0x02,
        Weapon = 0x04, // Has weapon and shield
        Base = 0x08, // This flag is set for every actor in Bethesda ESMs
        Swims = 0x10,
        Flies = 0x20, // Don't know what happens if several
        Walks = 0x40, // of these are set
        Essential = 0x80
    };
    
    public enum Type
    {
        Creatures = 0,
        Daedra = 1,
        Undead = 2,
        Humanoid = 3
    };

    public class NPDTstruct
    {
        public Type mType { get; set; } // int

        // For creatures we obviously have to use ints, not shorts and
        // bytes like we use for NPCs.... this file format just makes so
        // much sense! (Still, _much_ easier to decode than the NIFs.)
        public int mLevel { get; set; }
        public int[] mAttributes { get; } = new int[Attribute.sAttributes.Length];

        public int mHealth { get; set; } // Stats
        public int mMana { get; set; }
        public int mFatigue { get; set; }

        public int mSoul { get; set; } // The creatures soul value (used with soul gems.)

        // Creatures have generalized combat, magic and stealth stats which substitute for
        // the specific skills (in the same way as specializations).
        public int mCombat { get; set; }
        public int mMagic { get; set; }
        public int mStealth { get; set; }
        public int[] mAttack { get; } = new int[6]; // AttackMin1, AttackMax1, ditto2, ditto3
        public int mGold { get; set; }
    }; // 96 byte
    
    public override RecordName Name => RecordName.CREA;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mModel { get; set; }
    
    public RefId mOriginal { get; set; }
    
    public string mName { get; set; }
    
    public RefId mScript { get; set; }

    public NPDTstruct mData { get; set; } = new();
    
    public float mScale { get; set; }

    public InventoryList mInventory { get; set; } = new();

    public SpellList mSpells { get; set; } = new();

    public AIData mAiData { get; set; } = new();
    
    public AIPackageList mAiPackage { get; set; } = new();

    public Transport mTransport { get; set; } = new();
    
    public int mBloodType { get; set; }
    public byte mFlags { get; set; }

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        // mScale = 1.0f;
        // mAiData.mFight = 90;
        // mAiData.mFlee = 20;
        
        mSpells.mList.Clear();
        mInventory.mList.Clear();

        var hasName = false;
        var hasNpdt = false;
        var hasFlags = false;
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
                case RecordName.CNAM:
                    mOriginal = reader.getRefId();
                    break;
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();
                    break;
                case RecordName.NPDT:
                    reader.getHT(() =>
                    {
                        mData.mType = (Type)reader.BinaryReader.ReadInt32();
                        mData.mLevel = reader.BinaryReader.ReadInt32();
                        for (var i = 0; i < mData.mAttributes.Length; ++i)
                            mData.mAttributes[i] = reader.BinaryReader.ReadInt32();
                        mData.mHealth = reader.BinaryReader.ReadInt32();
                        mData.mMana = reader.BinaryReader.ReadInt32();
                        mData.mFatigue = reader.BinaryReader.ReadInt32();
                        mData.mSoul = reader.BinaryReader.ReadInt32();
                        mData.mCombat = reader.BinaryReader.ReadInt32();
                        mData.mMagic = reader.BinaryReader.ReadInt32();
                        mData.mStealth = reader.BinaryReader.ReadInt32();
                        for (var i = 0; i < mData.mAttack.Length; ++i)
                            mData.mAttack[i] = reader.BinaryReader.ReadInt32();
                        mData.mGold = reader.BinaryReader.ReadInt32();
                    });
                    hasNpdt = true;
                    break;
                case RecordName.FLAG:
                    reader.getHT(() =>
                    {
                        var flags = reader.BinaryReader.ReadInt32();
                        mFlags = (byte)(flags & 0xFF);
                        mBloodType = ((flags >> 8) & 0xFF) >> 2;
                    });
                    hasFlags = true;
                    break;
                case RecordName.XSCL:
                    reader.getHT(() => mScale = reader.BinaryReader.ReadSingle());
                    break;
                case RecordName.NPCO:
                    mInventory.Add(reader);
                    break;
                case RecordName.NPCS:
                    mSpells.Add(reader);
                    break;
                case RecordName.AIDT:
                    reader.getHT(() =>
                    {
                        mAiData.mHello = reader.BinaryReader.ReadUInt16();
                        mAiData.mFight = reader.BinaryReader.ReadByte();
                        mAiData.mFlee = reader.BinaryReader.ReadByte();
                        mAiData.mAlarm = reader.BinaryReader.ReadByte();
                        reader.skip(3); // padding
                        mAiData.mServices = (Services)reader.BinaryReader.ReadInt32();
                    });
                    break;
                case RecordName.DODT:
                case RecordName.DNAM:
                    mTransport.Add(reader);
                    break;
                case RecordName.AI_W:
                case RecordName.AI_A:
                case RecordName.AI_E:
                case RecordName.AI_F:
                case RecordName.AI_T:
                case RecordName.CNDT:
                    mAiPackage.Add(reader);
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                case RecordName.INDX:
                    // seems to occur only in .ESS files, unsure of purpose
                    int index = 0;
                    reader.getHT(() => index = reader.BinaryReader.ReadInt32());
                    Debug.WriteLine($"Creature::load: Unhandled INDX {index}");
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (!hasName)
            throw new MissingSubrecordException(RecordName.NAME);
        if (!hasNpdt && !isDeleted)
            throw new MissingSubrecordException(RecordName.NPDT);
        if (!hasFlags && !isDeleted)
            throw new MissingSubrecordException(RecordName.FLAG);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}