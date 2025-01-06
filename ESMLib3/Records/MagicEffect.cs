using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class MagicEffect : AbstractRecord
{
    private static readonly List<int> HardcodedFlags = [ 0x11c8, 0x11c0, 0x11c8, 0x11e0, 0x11e0, 0x11e0, 0x11e0,
        0x11d0, 0x11c0, 0x11c0, 0x11e0, 0x11c0, 0x11184, 0x11184, 0x1f0, 0x1f0, 0x1f0, 0x11d2, 0x11f0, 0x11d0,
        0x11d0, 0x11d1, 0x1d2, 0x1f0, 0x1d0, 0x1d0, 0x1d1, 0x1f0, 0x11d0, 0x11d0, 0x11d0, 0x11d0, 0x11d0, 0x11d0,
        0x11d0, 0x11d0, 0x11d0, 0x1d0, 0x1d0, 0x11c8, 0x31c0, 0x11c0, 0x11c0, 0x11c0, 0x1180, 0x11d8, 0x11d8,
        0x11d0, 0x11d0, 0x11180, 0x11180, 0x11180, 0x11180, 0x11180, 0x11180, 0x11180, 0x11180, 0x11c4, 0x111b8,
        0x1040, 0x104c, 0x104c, 0x104c, 0x104c, 0x1040, 0x1040, 0x1040, 0x11c0, 0x11c0, 0x1cc, 0x1cc, 0x1cc, 0x1cc,
        0x1cc, 0x1c2, 0x1c0, 0x1c0, 0x1c0, 0x1c1, 0x11c2, 0x11c0, 0x11c0, 0x11c0, 0x11c1, 0x11c0, 0x21192, 0x20190,
        0x20190, 0x20190, 0x21191, 0x11c0, 0x11c0, 0x11c0, 0x11c0, 0x11c0, 0x11c0, 0x11c0, 0x11c0, 0x11c0, 0x11c0,
        0x1c0, 0x11190, 0x9048, 0x9048, 0x9048, 0x9048, 0x9048, 0x9048, 0x9048, 0x9048, 0x9048, 0x9048, 0x9048,
        0x9048, 0x9048, 0x9048, 0x9048, 0x11c0, 0x1180, 0x1180, 0x5048, 0x5048, 0x5048, 0x5048, 0x5048, 0x5048,
        0x1188, 0x5048, 0x5048, 0x5048, 0x5048, 0x5048, 0x1048, 0x104c, 0x1048, 0x40, 0x11c8, 0x1048, 0x1048,
        0x1048, 0x1048, 0x1048, 0x1048 ];

    [Flags]
    public enum Flags : int
    {
        // Originally fixed flags (HardcodedFlags array consists of just these)
        TargetSkill = 0x1, // Affects a specific skill, which is specified elsewhere in the effect structure.

        TargetAttribute
            = 0x2, // Affects a specific attribute, which is specified elsewhere in the effect structure.
        NoDuration = 0x4, // Has no duration. Only runs effect once on cast.
        NoMagnitude = 0x8, // Has no magnitude.
        Harmful = 0x10, // Counts as a negative effect. Interpreted as useful for attack, and is treated as a bad

        // effect in alchemy.
        ContinuousVfx = 0x20, // The effect's hit particle VFX repeats for the full duration of the spell, rather

        // than occuring once on hit.
        CastSelf = 0x40, // Allows range - cast on self.
        CastTouch = 0x80, // Allows range - cast on touch.
        CastTarget = 0x100, // Allows range - cast on target.

        AppliedOnce
            = 0x1000, // An effect that is applied once it lands, instead of continuously. Allows an effect to reduce an

        // attribute below zero; removes the normal minimum effect duration of 1 second.
        Stealth = 0x2000, // Unused
        NonRecastable = 0x4000, // Does not land if parent spell is already affecting target. Shows "you cannot

        // re-cast" message for self target.
        IllegalDaedra = 0x8000, // Unused
        Unreflectable = 0x10000, // Cannot be reflected, the effect always lands normally.
        CasterLinked = 0x20000, // Must quench if caster is dead, or not an NPC/creature. Not allowed in
        // containter/door trap spells.

        // Originally modifiable flags
        AllowSpellmaking = 0x200, // Can be used for spellmaking
        AllowEnchanting = 0x400, // Can be used for enchanting
        NegativeLight = 0x800 // Inverts the effect's color
    }

    public class MEDTstruct
    {
        public RefId mSchool { get; set; } // Skill id
        
        public float mBaseCost { get; set; }

        public Flags mFlags { get; set; }   // int32

        // Glow color for enchanted items with this effect
        public int mRed { get; set; }
        
        public int mGreen { get; set; }
        
        public int mBlue { get; set; }

        public float mUnknown1 { get; set; } // Called "Size X" in CS
        
        public float mSpeed { get; set; } // Speed of fired projectile
        
        public float mUnknown2 { get; set; } // Called "Size Cap" in CS
    } // 36 bytes

    public static readonly List<RefId> sMagicSchools =
    [
        Skill.Alteration,
        Skill.Conjuration,
        Skill.Destruction,
        Skill.Illusion,
        Skill.Mysticism,
        Skill.Restoration,
    ];

    public override RecordName Name => RecordName.MGEF;

    public RefId mId { get; set; }

    public RecordFlag mRecordFlags { get; set; }

    public int mIndex { get; set; }

    public MEDTstruct mData { get; set; } = new();
    
    public string mIcon { get; set; }

    public string mParticle { get; set; }

    public string mDescription { get; set; }

    public RefId mArea { get; set; }

    public RefId mHit { get; set; }

    public RefId mBolt { get; set; }

    public RefId mCasting { get; set; }

    public RefId mAreaSound { get; set; }

    public RefId mHitSound { get; set; }

    public RefId mCastSound { get; set; }

    public RefId mBoltSound { get; set; }

    private RefId indexToRefId(int index)
    {
        if (index == -1)
            return new RefId();

        return RefId.Index(Name, (uint)index);
    }

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false; // MagicEffect record can't be deleted now (may be changed in the future)
        mRecordFlags = reader.getRecordFlags();

        reader.getHNT(RecordName.INDX, () => mIndex = reader.BinaryReader.ReadInt32());

        mId = indexToRefId(mIndex);

        reader.getSubNameIs(RecordName.MEDT);
        reader.getSubHeader();
        var school = reader.BinaryReader.ReadInt32();
        mData.mSchool = indexToSkillRefId(school);
        mData.mBaseCost = reader.BinaryReader.ReadSingle();
        mData.mFlags = (Flags)reader.BinaryReader.ReadInt32();
        mData.mRed = reader.BinaryReader.ReadInt32();
        mData.mGreen = reader.BinaryReader.ReadInt32();
        mData.mBlue = reader.BinaryReader.ReadInt32();
        mData.mUnknown1 = reader.BinaryReader.ReadSingle();
        mData.mSpeed = reader.BinaryReader.ReadSingle();
        mData.mUnknown2 = reader.BinaryReader.ReadSingle();

        if (reader.getFormatVersion() == FormatVersion.DefaultFormatVersion)
        {
            mData.mFlags &= (Flags.AllowSpellmaking | Flags.AllowEnchanting | Flags.NegativeLight);
            if (mIndex >= 0 && mIndex < HardcodedFlags.Count)
                mData.mFlags |= (Flags)HardcodedFlags[mIndex];
        }

        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.ITEX:
                    mIcon = reader.getHString();
                    break;
                case RecordName.PTEX:
                    mParticle = reader.getHString();
                    break;
                case RecordName.BSND:
                    mBoltSound = reader.getRefId();
                    break;
                case RecordName.CSND:
                    mCastSound = reader.getRefId();
                    break;
                case RecordName.HSND:
                    mHitSound = reader.getRefId();
                    break;
                case RecordName.ASND:
                    mAreaSound = reader.getRefId();
                    break;
                case RecordName.CVFX:
                    mCasting = reader.getRefId();
                    break;
                case RecordName.BVFX:
                    mBolt = reader.getRefId();
                    break;
                case RecordName.HVFX:
                    mHit = reader.getRefId();
                    break;
                case RecordName.AVFX:
                    mArea = reader.getRefId();
                    break;
                case RecordName.DESC:
                    mDescription = reader.getHString();
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        return;
    }

    private static RefId indexToSkillRefId(int index)
    {
        if (index < 0 || index >= sMagicSchools.Count)
            return new RefId();
        return sMagicSchools[index];
    }
    
    private static int skillRefIdToIndex(RefId id)
    {
        for (var i = 0; i < sMagicSchools.Count; ++i)
        {
            if (id == sMagicSchools[i])
                return i;
        }
        return -1;
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNT(RecordName.INDX, mIndex);

        writer.writeHNT(RecordName.MEDT, () =>
        {
            writer.Write(skillRefIdToIndex(mData.mSchool));
            writer.Write(mData.mBaseCost);
            writer.Write((int)mData.mFlags);
            writer.Write(mData.mRed);
            writer.Write(mData.mGreen);
            writer.Write(mData.mBlue);
            writer.Write(mData.mUnknown1);
            writer.Write(mData.mSpeed);
            writer.Write(mData.mUnknown2);            
        });

        writer.writeHNOCString(RecordName.ITEX, mIcon);
        writer.writeHNOCString(RecordName.PTEX, mParticle);
        writer.writeHNOCRefId(RecordName.BSND, mBoltSound);
        writer.writeHNOCRefId(RecordName.CSND, mCastSound);
        writer.writeHNOCRefId(RecordName.HSND, mHitSound);
        writer.writeHNOCRefId(RecordName.ASND, mAreaSound);

        writer.writeHNOCRefId(RecordName.CVFX, mCasting);
        writer.writeHNOCRefId(RecordName.BVFX, mBolt);
        writer.writeHNOCRefId(RecordName.HVFX, mHit);
        writer.writeHNOCRefId(RecordName.AVFX, mArea);

        writer.writeHNOString(RecordName.DESC, mDescription);
    }
}
