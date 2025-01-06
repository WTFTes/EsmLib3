using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;
using Attribute = EsmLib3.Structs.Attribute;

namespace EsmLib3.Records;

public class Npc : AbstractRecord
{
    [Flags]
    public enum Flags
    {
        None = 0,
        Female = 0x01,
        Essential = 0x02,
        Respawn = 0x04,
        Base = 0x08,
        Autocalc = 0x10
    }
    
    public enum NpcType
    {
        NPC_WITH_AUTOCALCULATED_STATS = 12,
        NPC_DEFAULT = 52
    };

    public class NPDTstruct52
    {
        public short mLevel { get; set; }

        public byte[] mAttributes { get; } = new byte[Attribute.sAttributes.Length];

        // mSkill can grow up to 200, it must be unsigned
        public byte[] mSkills { get; } = new byte[Skill.sSkills.Length];

        public sbyte mUnknown1 { get; set; }
        
        public ushort mHealth { get; set; }
        
        public ushort mMana { get; set; }
        
        public ushort mFatigue { get; set; }

        public byte mDisposition { get; set; }
        
        public byte mReputation { get; set; }
        
        public byte mRank { get; set; }

        public sbyte mUnknown2 { get; set; }
        
        public int mGold { get; set; }
    } // 52 bytes
    
    
    public override RecordName Name => RecordName.NPC_;
    
    public NpcType mNpdtType { get; set; }
    
    // Worth noting when saving the struct:
    //  Although we might read a NPDTstruct12 in, we use NPDTstruct52 internally
    public NPDTstruct52 mNpdt { get; set; } = new();
    
    public int mBloodType { get; set; }
    
    public byte mFlags { get; set; }

    public InventoryList mInventory { get; set; } = new();
    
    public SpellList mSpells { get; set; } = new();
    
    public AIData mAiData { get; set; } = new();

    public Transport mTransport { get; set; } = new();
    
    public AIPackageList mAIPackage { get; set; } = new();
    
    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mId { get; set; }
    
    public RefId mRace { get; set; }
    
    public RefId mClass { get; set; }
    
    public RefId mFaction { get; set; }
    
    public RefId mScript { get; set; }
    
    public string mModel { get; set; }
    
    public string mName { get; set; }
    
    public RefId mHair { get; set; }
    
    public RefId mHead { get; set; }
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        mSpells.mList.Clear();
        mInventory.mList.Clear();
        mTransport.mList.Clear();
        mAIPackage.mList.Clear();
        mAiData.Blank();
        // mAiData.mHello = mAiData.mFight = mAiData.mFlee = 30;
        
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
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.RNAM:
                    mRace = reader.getRefId();
                    break;
                case RecordName.CNAM:
                    mClass = reader.getRefId();
                    break;
                case RecordName.ANAM:
                    mFaction = reader.getRefId();
                    break;
                case RecordName.BNAM:
                    mHead = reader.getRefId();
                    break;
                case RecordName.KNAM:
                    mHair = reader.getRefId();
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();    
                    break;
                case RecordName.NPDT:
                    hasNpdt = true;
                    reader.getSubHeader();
                    if (reader.GetSubSize() == 52)
                    {
                        mNpdtType = NpcType.NPC_DEFAULT;
                        mNpdt.mLevel = reader.BinaryReader.ReadInt16();
                        for (var i = 0; i < mNpdt.mAttributes.Length; ++i)
                            mNpdt.mAttributes[i] = reader.BinaryReader.ReadByte();
                        for (var i = 0; i < mNpdt.mSkills.Length; ++i)
                            mNpdt.mSkills[i] = reader.BinaryReader.ReadByte();
                        mNpdt.mUnknown1 = reader.BinaryReader.ReadSByte();
                        mNpdt.mHealth = reader.BinaryReader.ReadUInt16();
                        mNpdt.mMana = reader.BinaryReader.ReadUInt16();
                        mNpdt.mFatigue = reader.BinaryReader.ReadUInt16();
                        mNpdt.mDisposition = reader.BinaryReader.ReadByte();
                        mNpdt.mReputation = reader.BinaryReader.ReadByte();
                        mNpdt.mRank = reader.BinaryReader.ReadByte();
                        mNpdt.mUnknown2 = reader.BinaryReader.ReadSByte();
                        mNpdt.mGold = reader.BinaryReader.ReadInt32();
                    }
                    else if (reader.GetSubSize() == 12)
                    {
                        mNpdtType = NpcType.NPC_WITH_AUTOCALCULATED_STATS;
                        
                        // Clearing the mNdpt struct to initialize all values
                        blankNpdt();
                        
                        mNpdt.mLevel = reader.BinaryReader.ReadInt16();
                        mNpdt.mDisposition = reader.BinaryReader.ReadByte();
                        mNpdt.mReputation = reader.BinaryReader.ReadByte();
                        mNpdt.mRank = reader.BinaryReader.ReadByte();
                        reader.skip(3);
                        mNpdt.mGold = reader.BinaryReader.ReadInt32();
                    }
                    else
                        throw new Exception("NPC_NPDT must be 12 or 52 bytes long");
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
                case RecordName.NPCS:
                    mSpells.Add(reader);
                    break;
                case RecordName.NPCO:
                    mInventory.Add(reader);
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
                    mAIPackage.Add(reader);
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
        if (!hasNpdt && !isDeleted)
            throw new MissingSubrecordException(RecordName.NPDT);
        if (!hasFlags && !isDeleted)
            throw new MissingSubrecordException(RecordName.FLAG);
    }

    private void blankNpdt()
    {
        mNpdt.mLevel = 0;
        Array.Fill(mNpdt.mAttributes, (byte)0);
        Array.Fill(mNpdt.mSkills, (byte)0);
        mNpdt.mReputation = 0;
        mNpdt.mHealth = mNpdt.mMana = mNpdt.mFatigue = 0;
        mNpdt.mDisposition = 0;
        mNpdt.mUnknown1 = 0;
        mNpdt.mRank = 0;
        mNpdt.mUnknown2 = 0;
        mNpdt.mGold = 0;
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        writer.writeHNOCString(RecordName.MODL, mModel);
        writer.writeHNOCString(RecordName.FNAM, mName);
        writer.writeHNCRefId(RecordName.RNAM, mRace);
        writer.writeHNCRefId(RecordName.CNAM, mClass);
        writer.writeHNCRefId(RecordName.ANAM, mFaction);
        writer.writeHNCRefId(RecordName.BNAM, mHead);
        writer.writeHNCRefId(RecordName.KNAM, mHair);
        writer.writeHNOCRefId(RecordName.SCRI, mScript);

        if (mNpdtType == NpcType.NPC_DEFAULT)
        {
            writer.writeHNT(RecordName.NPDT, () =>
            {
                writer.Write(mNpdt.mLevel);
                writer.Write(mNpdt.mAttributes);
                writer.Write(mNpdt.mSkills);
                writer.Write(mNpdt.mUnknown1);
                writer.Write(mNpdt.mHealth);
                writer.Write(mNpdt.mMana);
                writer.Write(mNpdt.mFatigue);
                writer.Write(mNpdt.mDisposition);
                writer.Write(mNpdt.mReputation);
                writer.Write(mNpdt.mRank);
                writer.Write(mNpdt.mUnknown2);
                writer.Write(mNpdt.mGold);
            });
        }
        else if (mNpdtType == NpcType.NPC_WITH_AUTOCALCULATED_STATS)
        {
            writer.writeHNT(RecordName.NPDT, () =>
            {
                writer.Write(mNpdt.mLevel);
                writer.Write(mNpdt.mDisposition);
                writer.Write(mNpdt.mReputation);
                writer.Write(mNpdt.mRank);
                writer.Write(new byte[3]);
                writer.Write(mNpdt.mGold);                
            });
        }

        writer.writeHNT(RecordName.FLAG, (int)((mBloodType << 10) + mFlags));

        mInventory.Save(writer);
        mSpells.Save(writer);
        writer.writeHNT(RecordName.AIDT, () =>
        {
            writer.Write(mAiData.mHello);
            writer.Write(mAiData.mFight);
            writer.Write(mAiData.mFlee);
            writer.Write(mAiData.mAlarm);
            writer.Write(new byte[3]);
            writer.Write((int)mAiData.mServices);
        });

        mTransport.Save(writer);

        mAIPackage.Save(writer);
    }
}