using System.Diagnostics;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Cell : AbstractRecord
{
    /* Moved cell reference tracking object. This mainly stores the target cell
            of the reference, so we can easily know where it has been moved when another
            plugin tries to move it independently.
        */
    public class MovedCellRef
    {
        public FormId mRefNum { get; set; } = new();

        // Coordinates of target exterior cell
        public int[] mTarget { get; } = new int[2];

        // The content file format does not support moving objects to an interior cell.
        // The save game format does support moving to interior cells, but uses a different mechanism
        // (see the MovedRefTracker implementation in MWWorld::CellStore for more details).
    }

    [Flags]
    public enum Flags
    {
        None = 0,
        Interior = 0x01, // Interior cell
        HasWater = 0x02, // Does this cell have a water surface
        NoSleep = 0x04, // Is it allowed to sleep here (without a bed)

        QuasiEx = 0x80 // Behave like exterior (Tribunal+), with
        // skybox and weather
    }

    public class DATAstruct
    {
        public Flags mFlags { get; set; } //int32
        public int mX { get; set; }
        public int mY { get; set; }
    }

    public class AMBIstruct
    {
        public uint mAmbient { get; set; }
        
        public uint mSunlight { get; set; }
        
        public uint mFog { get; set; }
        
        public float mFogDensity { get; set; }
    }

    public override RecordName Name => RecordName.CELL;

    public List<Context> mContextList { get; } = new();
    
    public RefId mId { get; set; }
    
    public string mName { get; set; }

    public DATAstruct mData { get; set; } = new();
    
    public bool mHasAmbi { get; set; }
    
    public bool mHasWaterHeightSub { get; set; }
    
    public float mWater { get; set; }

    public AMBIstruct mAmbi { get; set; } = new();
    
    public RefId mRegion { get; set; }
    
    public int mMapColor { get; set; }
    
    public int mRefNumCounter { get; set; }
    
    public List<Tuple<CellRef,bool>> mCellRefList { get; } = new();
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        loadNameAndData(reader, out isDeleted);
        loadCell(reader);
        return;
    }

    private void loadNameAndData(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        
        // blank();
        
        var hasData = false;
        var isLoaded = false;
        while (!isLoaded && reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mName = reader.getHString();
                    break;
                case RecordName.DATA:
                    reader.getHT(() =>
                    {
                        mData.mFlags = (Flags)reader.BinaryReader.ReadInt32();
                        mData.mX = reader.BinaryReader.ReadInt32();
                        mData.mY = reader.BinaryReader.ReadInt32();
                    });
                    hasData = true;
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                default:
                    reader.cacheSubName();
                    isLoaded = true;
                    break;
            }
        }

        if (!hasData)
            throw new MissingSubrecordException(RecordName.DATA);

        updateId();
    }

    private void loadCell(EsmReader reader)
    {
        var overriding = !string.IsNullOrEmpty(mName);
        mHasAmbi = false;
        mHasWaterHeightSub = false;

        CellRef cellRef = null;
        var refIsDeleted = false;

        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.INTV:
                    if (cellRef == null)
                    {
                        reader.getHT(() => { mWater = reader.BinaryReader.ReadInt32(); });
                        mHasWaterHeightSub = true;
                    }
                    else
                        reader.getHT(() => cellRef.mChargeInt = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.WHGT:
                    float waterLevel = 0;
                    reader.getHT(() => waterLevel = reader.BinaryReader.ReadSingle());
                    mHasWaterHeightSub = true;
                    if (!float.IsFinite(waterLevel))
                    {
                        if (!overriding)
                            mWater = float.MaxValue;
                        Debug.WriteLine(
                            $"Warning: Encountered invalid water level in cell {mName} defined in {reader.getContext().FileName}");
                    }
                    else
                        mWater = waterLevel;

                    break;
                case RecordName.AMBI:
                    reader.getHT(() =>
                    {
                        mAmbi.mAmbient = reader.BinaryReader.ReadUInt32();
                        mAmbi.mSunlight = reader.BinaryReader.ReadUInt32();
                        mAmbi.mFog = reader.BinaryReader.ReadUInt32();
                        mAmbi.mFogDensity = reader.BinaryReader.ReadSingle();
                    });
                    mHasAmbi = true;
                    break;
                case RecordName.RGNN:
                    mRegion = reader.getRefId();
                    break;
                case RecordName.NAM5:
                    reader.getHT(() => mMapColor = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.NAM0:
                    if (cellRef != null)
                        reader.skipHSub();
                    else
                        reader.getHT(() => mRefNumCounter = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.FRMR:
                    if (cellRef != null)
                        mCellRefList.Add(new(cellRef, refIsDeleted));

                    cellRef = new CellRef();
                    refIsDeleted = false;

                    // cellRef.blank();
                    var wide = false;
                    if (wide)
                    {
                        reader.getHT(() =>
                        {
                            cellRef.mRefNum.Index = reader.BinaryReader.ReadUInt32();
                            cellRef.mRefNum.ContentFile = reader.BinaryReader.ReadInt32();
                        });
                    }
                    else
                    {
                        reader.getHT(() => cellRef.mRefNum.Index = reader.BinaryReader.ReadUInt32());
                    }

                    cellRef.mRefId = reader.getHNORefId(RecordName.NAME);
                    break;
                case RecordName.XSCL:
                    reader.getHT(() => { cellRef.mScale = reader.BinaryReader.ReadSingle(); });
                    break;
                case RecordName.DATA:
                    // cell data already loaded before
                    reader.getHT(() =>
                    {
                        cellRef.mPos.X = reader.BinaryReader.ReadSingle();
                        cellRef.mPos.Y = reader.BinaryReader.ReadSingle();
                        cellRef.mPos.Z = reader.BinaryReader.ReadSingle();
                        cellRef.mPos.RotX = reader.BinaryReader.ReadSingle();
                        cellRef.mPos.RotY = reader.BinaryReader.ReadSingle();
                        cellRef.mPos.RotZ = reader.BinaryReader.ReadSingle();
                    });
                    break;
                case RecordName.DODT:
                    reader.getHT(() =>
                    {
                        cellRef.mDoorDest.X = reader.BinaryReader.ReadSingle();
                        cellRef.mDoorDest.Y = reader.BinaryReader.ReadSingle();
                        cellRef.mDoorDest.Z = reader.BinaryReader.ReadSingle();
                        cellRef.mDoorDest.RotX = reader.BinaryReader.ReadSingle();
                        cellRef.mDoorDest.RotY = reader.BinaryReader.ReadSingle();
                        cellRef.mDoorDest.RotZ = reader.BinaryReader.ReadSingle();
                    });
                    cellRef.mTeleport = true;
                    break;
                case RecordName.DNAM:
                    cellRef.mDestCell = reader.getHString();
                    break;
                case RecordName.ANAM:
                    cellRef.mOwner = reader.getRefId();
                    break;
                case RecordName.NAM9:
                    reader.getHT(() => cellRef.mCount = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.FLTV:
                    reader.getHT(() => cellRef.mLockLevel = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.KNAM:
                    cellRef.mKey = reader.getRefId();
                    break;
                case RecordName.CNAM:
                    cellRef.mFaction = reader.getRefId();
                    break;
                case RecordName.INDX:
                    reader.getHT(() => cellRef.mFactionRank = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.TNAM:
                    cellRef.mTrap = reader.getRefId();
                    break;
                case RecordName.XSOL:
                    cellRef.mSoul = reader.getRefId();
                    break;
                case RecordName.BNAM:
                    cellRef.mGlobalVariable = reader.getHString();
                    break;
                case RecordName.XCHG:
                    reader.getHT(() => cellRef.mEnchantmentCharge = reader.BinaryReader.ReadSingle());
                    break;
                case RecordName.UNAM:
                    reader.getHT(() => cellRef.mReferenceBlocked = reader.BinaryReader.ReadSByte());
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    refIsDeleted = true;
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (cellRef != null)
            mCellRefList.Add(new(cellRef, refIsDeleted));
    }

    private void updateId()
    {
        mId = generateIdForCell((mData.mFlags & Flags.Interior) == 0, mName, mData.mX, mData.mY);
    }

    private RefId generateIdForCell(bool exterior, string cellName, int x, int y)
    {
        if (!exterior)
            return RefId.StringRefId(cellName);

        return RefId.Esm3ExteriorCell(x, y);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}