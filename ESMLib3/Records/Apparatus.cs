﻿using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Apparatus : AbstractRecord
{
    public enum AppaType : int
    {
        MortarPestle = 0,
        Alembic = 1,
        Calcinator = 2,
        Retort = 3
    }

    public class AADTstruct
    {
        public AppaType mType { get; set; } // int32
        
        public float mQuality { get; set; }
        
        public float mWeight { get; set; }
        
        public int mValue { get; set; }
    }

    public override RecordName Name { get; }
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public RefId mScript { get; set; }
    
    public string mName { get; set; }
    
    public string mModel { get; set; }
    
    public string mIcon { get; set; }

    public AADTstruct mData { get; set; } = new();

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        var hasName = false;
        var hasData = false;
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
                case RecordName.AADT:
                    reader.getHT(() =>
                    {
                        mData.mType = (AppaType)reader.BinaryReader.ReadInt32();
                        mData.mQuality = reader.BinaryReader.ReadSingle();
                        mData.mWeight = reader.BinaryReader.ReadSingle();
                        mData.mValue = reader.BinaryReader.ReadInt32();
                    });
                    hasData = true;
                    break;
                case RecordName.SCRI:
                    mScript = reader.getRefId();
                    break;
                case RecordName.ITEX:
                    mIcon = reader.getHString();
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
        if (!hasData && !isDeleted)
            throw new MissingSubrecordException(RecordName.AADT);
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        writer.writeHNCString(RecordName.MODL, mModel);
        writer.writeHNCString(RecordName.FNAM, mName);
        writer.writeHNT(RecordName.AADT, () =>
        {
            writer.Write((int)mData.mType);
            writer.Write(mData.mQuality);
            writer.Write(mData.mWeight);
            writer.Write(mData.mValue);
        });
        writer.writeHNOCRefId(RecordName.SCRI, mScript);
        writer.writeHNCString(RecordName.ITEX, mIcon);
    }
}