using System.Diagnostics;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;
using EsmLib3.Structs;

namespace EsmLib3.Records;

public class Cell : AbstractRecord
{
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
    
    // Counter for RefNums. This is only used during content file editing and has no impact on gameplay.
    // It prevents overwriting previous refNums, even if they were deleted.
    // as that would collide with refs when a content file is upgraded.
    public int mRefNumCounter { get; set; }

    public List<Tuple<CellRef, bool>>[] mCellRefList { get; } = [[], []];

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        loadNameAndData(reader, out isDeleted);
        loadCell(reader);
        loadRefs(reader);
    }

    private void loadNameAndData(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        
        blank();
        
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

        var isLoaded = false;
        while (!isLoaded && reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.INTV:
                    reader.getHT(() => { mWater = reader.BinaryReader.ReadInt32(); });
                    mHasWaterHeightSub = true;
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
                default:
                    reader.cacheSubName();
                    isLoaded = true;
                    break;
            }
        }
    }

    private void loadRefs(EsmReader reader)
    {
        CellRef? cellRef = null;
        MovedCellRef? movedCellRef = null;
        var refIsDeleted = false;
        var cellRefList = 0;

        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.FRMR:
                    if (cellRef != null)
                        AddCellRef(reader, cellRef, cellRefList, refIsDeleted);

                    cellRef = new CellRef();
                    if (movedCellRef != null)
                    {
                        cellRef.mMovedCell = movedCellRef;
                        movedCellRef = null;
                    }

                    refIsDeleted = false;

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
                        reader.getHT(() => cellRef.mRefNum.Index = reader.BinaryReader.ReadUInt32());

                    break;
                case RecordName.NAME:
                    cellRef.mRefId = reader.getRefId();
                    break;
                case RecordName.INTV:
                    reader.getHT(() => cellRef.mChargeInt = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.NAM0:
                    if (cellRef != null)
                    {
                        AddCellRef(reader, cellRef, cellRefList, refIsDeleted);
                        refIsDeleted = false;
                        cellRef = null;
                    }

                    if (cellRefList >= 1)
                        throw new Exception("Can't be more than one NAM0 within CELL structure");
                    ++cellRefList;
                    reader.getHT(() => mRefNumCounter = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    refIsDeleted = true;
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
                case RecordName.MVRF:
                    // MVRF are FRMR are present in pairs. MVRF indicates that following FRMR describes moved CellRef.
                    if (movedCellRef != null)
                        throw new Exception("Previous moved cell ref is not null");

                    movedCellRef = new();
                    FormId id = new();
                    reader.getHT(() => id.Index = reader.BinaryReader.ReadUInt32());
                    reader.getHNOT(RecordName.CNDT, () =>
                    {
                        for (var i = 0; i < movedCellRef.mTarget.Length; ++i)
                            movedCellRef.mTarget[i] = reader.BinaryReader.ReadInt32();
                    });

                    movedCellRef.mRefNum = id;

                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (movedCellRef != null)
            throw new Exception("Moved cell ref without cell itself");

        if (cellRef != null)
            AddCellRef(reader, cellRef, cellRefList, refIsDeleted);
    }

    private void AddCellRef(EsmReader reader, CellRef cellRef, int cellRefList, bool isDeleted)
    {
        cellRef.mRefNum = adjustRefNum(cellRef.mRefNum, reader);

        if (reader.getFormatVersion() == FormatVersion.DefaultFormatVersion) // loading a content file
            cellRef.mIsLocked = !cellRef.mKey.IsEmpty || cellRef.mLockLevel > 0;
        else
            cellRef.mIsLocked = cellRef.mLockLevel > 0;

        if (cellRef.mLockLevel == CellRef.ZeroLock)
            cellRef.mLockLevel = 0;

        mCellRefList[cellRefList].Add(new(cellRef, isDeleted));
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

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCString(RecordName.NAME, mName);
        writer.writeHNT(RecordName.DATA, () =>
        {
            writer.Write((int)mData.mFlags);
            writer.Write(mData.mX);
            writer.Write(mData.mY);
        });

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        if (mData.mFlags.HasFlag(Flags.Interior))
        {
            // Try to avoid saving ambient information when it's unnecessary.
            // This is to fix black lighting and flooded water
            // in resaved cell records that lack this information.
            if (mHasWaterHeightSub)
                writer.writeHNT(RecordName.WHGT, mWater);
            if (mData.mFlags.HasFlag(Flags.QuasiEx))
                writer.writeHNOCRefId(RecordName.RGNN, mRegion);
            else if (mHasAmbi)
                writer.writeHNT(RecordName.AMBI, () =>
                {
                    writer.Write(mAmbi.mAmbient);
                    writer.Write(mAmbi.mSunlight);
                    writer.Write(mAmbi.mFog);
                    writer.Write(mAmbi.mFogDensity);
                });
        }
        else
        {
            writer.writeHNOCRefId(RecordName.RGNN, mRegion);
            if (mMapColor != 0)
                writer.writeHNT(RecordName.NAM5, mMapColor);
        }

        // write persistent
        WriteReferences(writer, mCellRefList[0]);
        
        // count of total refs - temp refs
        if (mRefNumCounter > 0)
            writer.writeHNT(RecordName.NAM0, mRefNumCounter);

        // write tmp
        WriteReferences(writer, mCellRefList[1]);
    }

    private void WriteReferences(EsmWriter writer, List<Tuple<CellRef, bool>> mCellRefs)
    {
        foreach (var (r, refDeleted) in mCellRefs)
        {
            if (r.mMovedCell != null)
            {
                writer.writeFormId(r.mMovedCell.mRefNum, false, RecordName.MVRF);
                foreach (var c in r.mMovedCell.mTarget)
                    writer.writeHNT(RecordName.CNDT, c);
            }

            r.Save(writer, false, false, refDeleted);
        }
    }

    private void blank()
    {
        mName = "";
        mRegion = new RefId();
        mWater = 0;
        mMapColor = 0;
        mRefNumCounter = 0;

        mData.mFlags = 0;
        mData.mX = 0;
        mData.mY = 0;

        mHasAmbi = true;
        mHasWaterHeightSub = true;
        mAmbi.mAmbient = 0;
        mAmbi.mSunlight = 0;
        mAmbi.mFog = 0;
        mAmbi.mFogDensity = 0;
    }
    
    // Translate 8bit/24bit code (stored in refNum.mIndex) into a proper refNum
    private FormId adjustRefNum(FormId refNum, EsmReader reader)
    {
        var local = (refNum.Index & 0xff000000) >> 24;

        // If we have an index value that does not make sense, assume that it was an addition
        // by the present plugin (but a faulty one)
        if (local > 0 && local <= reader.getParentFileIndices().Count)
        {
            // If the most significant 8 bits are used, then this reference already exists.
            // In this case, do not spawn a new reference, but overwrite the old one.
            refNum.Index &= 0x00ffffff; // delete old plugin ID
            refNum.ContentFile = reader.getParentFileIndices()[(int)local - 1];
        }
        else
        {
            // This is an addition by the present plugin. Set the corresponding plugin index.
            refNum.ContentFile = reader.getIndex();
        }

        return refNum;
    }
}