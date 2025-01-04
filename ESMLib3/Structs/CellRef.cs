using EsmLib3.RefIds;

namespace EsmLib3.Structs;

/* Cell reference. This represents ONE object (of many) inside the
    cell. The cell references are not loaded as part of the normal
    loading process, but are rather loaded later on demand when we are
    setting up a specific cell.
    */
public class CellRef
{
    // Reference number
    // Note: Currently unused for items in containers
    public FormId mRefNum { get; set; } = new();

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

    public void blank()
    {
        mRefNum = new FormId();
        mRefId = new EmptyRefId();
        mScale = 1;
        mOwner = new EmptyRefId();
        mGlobalVariable = "";
        mSoul = new EmptyRefId();
        mFaction = new EmptyRefId();
        mFactionRank = -2;
        mChargeInt = -1;
        mEnchantmentCharge = -1;
        mCount = 1;
        mDestCell = "";
        mLockLevel = 0;
        mIsLocked = false;
        mKey = new EmptyRefId();
        mTrap = new EmptyRefId();
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
}