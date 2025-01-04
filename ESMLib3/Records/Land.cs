using EsmLib3.Exceptions;

namespace EsmLib3.Records;

public class Land : AbstractRecord
{
    public class VHGT
    {
        public float mHeightOffset { get; set; }

        public sbyte[] mHeightData { get; } = new sbyte[sLandNumVerts];

        public ushort mUnk1 { get; set; }

        public sbyte mUnk2 { get; set; }
    }

    private const uint LAND_GLOBAL_MAP_LOD_SIZE = 81;
    
    // number of vertices per side
    private const uint sLandSize = 65;

    // total number of vertices
    public const uint sLandNumVerts = sLandSize * sLandSize;

    // number of textures per side of land
    private const uint sLandTextureSize = 16;

    // total number of textures per land
    private const uint sLandNumTextures = sLandTextureSize * sLandTextureSize;

    public override RecordName Name => RecordName.LAND;
    
    public int mX { get; set; }
    
    public int mY { get; set; }
    
    public uint mFlags { get; set; }

    public VHGT mVHGT { get; set; } = new();
    
    // low-LOD heightmap (used for rendering the global map)
    public sbyte[] mWnam { get; }= new sbyte[LAND_GLOBAL_MAP_LOD_SIZE]; 
    
    // 24-bit normals, these aren't always correct though. Edge and corner normals may be garbage.
    public sbyte[] mNormals { get; } = new sbyte[sLandNumVerts * 3];
    
    // 24-bit RGB color for each vertex
    public byte[] mColours { get; } = new byte[3 * sLandNumVerts];
    
    // 2D array of texture indices
    public ushort[] vTex { get; } = new ushort[sLandNumTextures];

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;

        var hasLocation = false;
        var isLoaded = false;
        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.INTV:
                    reader.getSubHeader();
                    if (reader.GetSubSize() != 8)
                        throw new Exception("Subrecord size is not equal to 8");

                    mX = reader.BinaryReader.ReadInt32();
                    mY = reader.BinaryReader.ReadInt32();

                    hasLocation = true;
                    break;
                case RecordName.DATA:
                    reader.getHT(() => mFlags = reader.BinaryReader.ReadUInt32());
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                case RecordName.VNML:
                    reader.getHT(() =>
                    {
                        for (var i = 0; i < mNormals.Length; ++i)
                            mNormals[i] = reader.BinaryReader.ReadSByte();
                    });
                    break;
                case RecordName.VHGT:
                    reader.getHT(() =>
                    {
                        mVHGT.mHeightOffset = reader.BinaryReader.ReadSingle();
                        for (var i = 0; i < mVHGT.mHeightData.Length; ++i)
                            mVHGT.mHeightData[i] = reader.BinaryReader.ReadSByte();
                        mVHGT.mUnk1 = reader.BinaryReader.ReadUInt16();
                        mVHGT.mUnk2 = reader.BinaryReader.ReadSByte();
                    });
                    break;
                case RecordName.WNAM:
                    reader.getHT(() =>
                    {
                        for (var i = 0; i < mWnam.Length; ++i)
                            mWnam[i] = reader.BinaryReader.ReadSByte();
                    });
                    break;
                case RecordName.VCLR:
                    reader.getHT(() =>
                    {
                        for (var i = 0; i< mColours.Length;++i)
                            mColours[i] = reader.BinaryReader.ReadByte();
                    });
                    break;
                case RecordName.VTEX:
                    reader.getHT(() =>
                    {
                        for (var i = 0; i< vTex.Length;++i)
                            vTex[i] = reader.BinaryReader.ReadUInt16();
                    });
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        if (!hasLocation)
            throw new MissingSubrecordException(RecordName.INTV);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}