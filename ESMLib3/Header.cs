namespace EsmLib3;

public class Header
{
    public enum DataType : int
    {
        Esp = 0,
        Esm = 1,
        Ess = 2,
    }

    public struct Data
    {
        public RecordName version;
        public DataType type; // 0=esp, 1=esm, 32=ess (unused)
        public string author; // Author's name
        public string desc; // File description
        public int records; // Number of records
    };
    
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
    
    struct MasterData
    {
        public string name;
        public ulong size;
    };

    public Data mData;

    public FormatVersion FormatVersion = FormatVersion.DefaultFormatVersion;
    private List<MasterData> mMaster = new();
    
    private GMDT mGameData;
    private byte[] mSCRD;
    private byte[] mSCRS;

    public void Load(EsmReader reader)
    {
        var res =reader.getHNOT(RecordName.FORM, () => FormatVersion = (FormatVersion)reader.BinaryReader.ReadUInt32());

        if (reader.IsNextSub(RecordName.HEDR))
        {
            reader.getSubHeader();
            mData.version = (RecordName)reader.BinaryReader.ReadUInt32();
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

        reader.getHNOT(RecordName.GMDT, () =>
        {
            mGameData = new();
            mGameData.mCurrentHealth = reader.BinaryReader.ReadSingle();
            mGameData.mMaximumHealth = reader.BinaryReader.ReadSingle();
            mGameData.mHour = reader.BinaryReader.ReadSingle();
            mGameData.unknown1 = reader.BinaryReader.ReadBytes(12);
            mGameData.mCurrentCell = reader.BinaryReader.ReadBytes(64);
            mGameData.unknown2 = reader.BinaryReader.ReadBytes(4);
            mGameData.mPlayerName = reader.BinaryReader.ReadBytes(32);
        });

        if (reader.IsNextSub(RecordName.SCRD))
        {
            reader.getSubHeader();
            if (reader.GetSubSize() > 0)
                mSCRD = reader.BinaryReader.ReadBytes((int)reader.GetSubSize());
        }

        if (reader.IsNextSub(RecordName.SCRS))
        {
            reader.getSubHeader();
            if (reader.GetSubSize() > 0)
                mSCRS = reader.BinaryReader.ReadBytes((int)reader.GetSubSize());
        }
    }

    public void blank()
    {
        mData.version = RecordName.TES3;
        mData.type = 0;
        mData.author = "";
        mData.desc = "";
        mData.records = 0;
        FormatVersion = FormatVersion.CurrentContentFormatVersion;
        mMaster.Clear();
    }
}
