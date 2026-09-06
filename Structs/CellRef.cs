using EsmLib3.RefIds;

namespace EsmLib3.Structs;

/* Cell reference. This represents ONE object (of many) inside the
    cell. The cell references are not loaded as part of the normal
    loading process, but are rather loaded later on demand when we are
    setting up a specific cell.
    */
public class CellRef
{
    public CellRef()
    {
        blank();
    }

    public const int ZeroLock = int.MaxValue;

    // Reference number
    // Note: Currently unused for items in containers
    public FormId mRefNum;

    public RefId mRefId { get; set; } // ID of object being referenced

    public float mScale { get; set; } // Scale applied to mesh

    // The NPC that owns this object (and will get angry if you steal it)
    public RefId mOwner { get; set; }

    // Name of a global variable. If the global variable is set to '1', using the object is temporarily allowed
    // even if it has an Owner field.
    // Used by bed rent scripts to allow the player to use the bed for the duration of the rent.
    public string mGlobalVariable { get; set; }

    // ID of creature trapped in this soul gem
    public RefId mSoul { get; set; }

    // The faction that owns this object (and will get angry if
    // you take it and are not a faction member)
    public RefId mFaction { get; set; }

    // PC faction rank required to use the item. Sometimes is -1, which means "any rank".
    public int mFactionRank { get; set; }

    // For weapon or armor, this is the remaining item health.
    // For tools (lockpicks, probes, repair hammer) it is the remaining uses.
    // For lights it is remaining time.
    // This could be -1 if the charge was not touched yet (i.e. full).
    public int mChargeInt { get; set; } // Float for lights

    // Remaining enchantment charge. This could be -1 if the charge was not touched yet (i.e. full).
    public float mEnchantmentCharge { get; set; }

    public int mCount { get; set; }

    // For doors - true if this door teleports to somewhere else, false
    // if it should open through animation.
    public bool mTeleport { get; set; }

    // Teleport location for the door, if this is a teleporting door.
    public Position mDoorDest { get; set; } = new();

    // Destination cell for doors (optional)
    public string mDestCell { get; set; }

    // Lock level for doors and containers
    public int mLockLevel { get; set; }

    public bool mIsLocked { get; set; }

    public RefId mKey { get; set; } // Key and trap ID names, if any

    public RefId mTrap { get; set; }

    // This corresponds to the "Reference Blocked" checkbox in the construction set,
    // which prevents editing that reference.
    // -1 is not blocked, otherwise it is blocked.
    public sbyte mReferenceBlocked { get; set; }

    // Position and rotation of this object within the cell
    public Position mPos { get; set; } = new();

    public MovedCellRef mMovedCell { get; set; }

    public void blank()
    {
        mRefNum = new FormId();
        mRefId = new RefId();
        mScale = 1;
        mOwner = new RefId();
        mGlobalVariable = "";
        mSoul = new RefId();
        mFaction = new RefId();
        mFactionRank = -2;
        mChargeInt = -1;
        mEnchantmentCharge = -1;
        mCount = 1;
        mDestCell = "";
        mLockLevel = 0;
        mIsLocked = false;
        mKey = new RefId();
        mTrap = new RefId();
        mReferenceBlocked = -1;
        mTeleport = false;

        mDoorDest.X = 0;
        mDoorDest.Y = 0;
        mDoorDest.Z = 0;
        mDoorDest.RotX = 0;
        mDoorDest.RotY = 0;
        mDoorDest.RotZ = 0;

        mPos.X = 0;
        mPos.Y = 0;
        mPos.Z = 0;
        mPos.RotX = 0;
        mPos.RotY = 0;
        mPos.RotZ = 0;
    }

    public void Save(EsmWriter writer, bool wideRefNum, bool inInventory, bool isDeleted)
    {
        writer.writeFormId(mRefNum, wideRefNum);

        writer.writeHNCRefId(RecordName.NAME, mRefId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        if (mScale != 1.0)
            writer.writeHNT(RecordName.XSCL, (float)Math.Clamp(mScale, 0.5f, 2.0f)); // 0.5 to 2.0

        if (!inInventory)
            writer.writeHNOCRefId(RecordName.ANAM, mOwner);

        writer.writeHNOCString(RecordName.BNAM, mGlobalVariable);
        writer.writeHNOCRefId(RecordName.XSOL, mSoul);

        if (!inInventory)
        {
            writer.writeHNOCRefId(RecordName.CNAM, mFaction);
            if (mFactionRank != -2)
            {
                writer.writeHNT(RecordName.INDX, mFactionRank);
            }
        }

        if (mEnchantmentCharge != -1)
            writer.writeHNT(RecordName.XCHG, mEnchantmentCharge);

        if (mChargeInt != -1)
            writer.writeHNT(RecordName.INTV, mChargeInt);

        if (mCount != 1)
            writer.writeHNT(RecordName.NAM9, mCount);

        if (!inInventory && mTeleport)
        {
            writer.writeHNT(RecordName.DODT, () =>
            {
                writer.Write(mDoorDest.X);
                writer.Write(mDoorDest.Y);
                writer.Write(mDoorDest.Z);
                writer.Write(mDoorDest.RotX);
                writer.Write(mDoorDest.RotY);
                writer.Write(mDoorDest.RotZ);
            });
            writer.writeHNOCString(RecordName.DNAM, mDestCell);
        }

        if (!inInventory)
        {
            if (mIsLocked)
            {
                var lockLevel = mLockLevel;
                if (lockLevel == 0)
                    lockLevel = ZeroLock;
                writer.writeHNT(RecordName.FLTV, lockLevel);
                writer.writeHNOCRefId(RecordName.KNAM, mKey);
            }

            writer.writeHNOCRefId(RecordName.TNAM, mTrap);
        }

        if (mReferenceBlocked != -1)
            writer.writeHNT(RecordName.UNAM, mReferenceBlocked);

        if (!inInventory)
            writer.writeHNT(RecordName.DATA, () =>
            {
                writer.Write(mPos.X);
                writer.Write(mPos.Y);
                writer.Write(mPos.Z);
                writer.Write(mPos.RotX);
                writer.Write(mPos.RotY);
                writer.Write(mPos.RotZ);
            });
    }
}