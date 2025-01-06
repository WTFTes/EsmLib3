using EsmLib3.Enums;

namespace EsmLib3;

public struct Header
{
    public Header()
    {
    }
   
    public enum DataType : int
    {
        Esp = 0,
        Esm = 1,
        Ess = 2,
    }

    public struct Data
    {
        public EsmVersion version;
        public DataType type; // 0=esp, 1=esm, 32=ess (unused)
        public string author; // Author's name
        public string desc; // File description
        public int records; // Number of records
    }
    
    public struct GMDT
    {
        public float mCurrentHealth;
        public float mMaximumHealth;
        public float mHour;
        public byte[] unknown1;//12
        public byte[] mCurrentCell;//64
        public byte[] unknown2; // 4
        public byte[] mPlayerName;//32
    };
   
    // Defines another files (esm or esp) that this file depends upon.
    struct MasterData
    {
        public string name;
        public ulong size;
    };

    public Data mData;

    public FormatVersion FormatVersion = FormatVersion.DefaultFormatVersion;
    private List<MasterData> mMaster = new();
    
    private GMDT mGameData; // Used in .ess savegames only
    
    private byte[]? mSCRD; // Used in .ess savegames only, unknown
    
    private byte[]? mSCRS; // Used in .ess savegames only, screenshot

    public void Load(EsmReader reader)
    {
        var version = FormatVersion.DefaultFormatVersion;
        reader.getHNOT(RecordName.FORM, () => version = (FormatVersion)reader.BinaryReader.ReadUInt32());
        FormatVersion = version;

        if (reader.IsNextSub(RecordName.HEDR))
        {
            reader.getSubHeader();
            mData.version = (EsmVersion)reader.BinaryReader.ReadUInt32();
            mData.type = (DataType)reader.BinaryReader.ReadInt32();
            mData.author = reader.getMaybeFixedStringSize(32);
            mData.desc = reader.getMaybeFixedStringSize(256);
            mData.records = reader.BinaryReader.ReadInt32();
        }

        while (reader.IsNextSub(RecordName.MAST))
        {
            MasterData m = new();
            m.name = reader.getHString();
            reader.getHNT(RecordName.DATA, () => m.size = reader.BinaryReader.ReadUInt64());
            mMaster.Add(m);
        }

        if (reader.IsNextSub(RecordName.GMDT))
        {
            reader.getSubHeader();
            
            GMDT gmdt = new();
            gmdt.mCurrentHealth = reader.BinaryReader.ReadSingle();
            gmdt.mMaximumHealth = reader.BinaryReader.ReadSingle();
            gmdt.mHour = reader.BinaryReader.ReadSingle();
            gmdt.unknown1 = reader.BinaryReader.ReadBytes(12);
            gmdt.mCurrentCell = reader.BinaryReader.ReadBytes(64);
            gmdt.unknown2 = reader.BinaryReader.ReadBytes(4);
            gmdt.mPlayerName = reader.BinaryReader.ReadBytes(32);
            mGameData = gmdt;
        }

        if (reader.IsNextSub(RecordName.SCRD))
        {
            reader.getSubHeader();
            if (reader.GetSubSize() > 0)
                mSCRD = reader.BinaryReader.ReadBytes((int)reader.GetSubSize());
            else
                mSCRD = [];
        }

        if (reader.IsNextSub(RecordName.SCRS))
        {
            reader.getSubHeader();
            if (reader.GetSubSize() > 0)
                mSCRS = reader.BinaryReader.ReadBytes((int)reader.GetSubSize());
            else
                mSCRS = [];
        }
    }

    public void Save(EsmWriter writer)
    {
        if (FormatVersion > FormatVersion.DefaultFormatVersion)
            writer.writeHNT(RecordName.FORM, (int)FormatVersion);
        
        writer.startSubRecord(RecordName.HEDR);
        writer.Write((int)mData.version);
        writer.Write((int)mData.type);
        writer.writeMaybeFixedSizeString(mData.author, 32);
        writer.writeMaybeFixedSizeString(mData.desc, 256);
        writer.Write(mData.records);
        writer.endRecord(RecordName.HEDR);
        
        foreach (var data in mMaster)
        {
            writer.writeHNCString(RecordName.MAST, data.name);
            writer.writeHNT(RecordName.DATA, data.size);
        }

        if (mData.type == DataType.Ess)
        {
            writer.startSubRecord(RecordName.GMDT);
            writer.Write(mGameData.mCurrentHealth);
            writer.Write(mGameData.mMaximumHealth);
            writer.Write(mGameData.mHour);
            writer.Write(mGameData.unknown1, 12);
            writer.Write(mGameData.mCurrentCell, 64);
            writer.Write(mGameData.unknown2, 4);
            writer.Write(mGameData.mPlayerName, 32);
            writer.endRecord(RecordName.GMDT);
            
            writer.writeHNT(RecordName.SCRD, mSCRD ?? []);
            writer.writeHNT(RecordName.SCRS, mSCRS ?? []);
        }
    }

    public void blank()
    {
        mData.version = EsmVersion.VER_130;
        mData.type = 0;
        mData.author = "";
        mData.desc = "";
        mData.records = 0;
        FormatVersion = FormatVersion.CurrentContentFormatVersion;
        mMaster.Clear();
        mSCRD = null;
        mSCRS = null;
    }
}
