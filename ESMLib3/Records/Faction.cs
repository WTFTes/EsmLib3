using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class Faction : AbstractRecord
{
    // Requirements for each rank
    public struct RankData
    {
        public int mAttribute1, mAttribute2; // Attribute level

        // Skill level (faction skills given in
        // skillID below.) You need one skill at
        // level 'mPrimarySkill' and two skills at level
        // 'mFavouredSkill' to advance to this rank.
        public int mPrimarySkill, mFavouredSkill;

        public int mFactReaction; // Reaction from faction members

        public void Load(EsmReader reader)
        {
            mAttribute1 = reader.BinaryReader.ReadInt32();
            mAttribute2 = reader.BinaryReader.ReadInt32();
            mPrimarySkill = reader.BinaryReader.ReadInt32();
            mFavouredSkill = reader.BinaryReader.ReadInt32();
            mFactReaction = reader.BinaryReader.ReadInt32();
        }

        public void Save(EsmWriter writer)
        {
            writer.Write(mAttribute1);
            writer.Write(mAttribute2);
            writer.Write(mPrimarySkill);
            writer.Write(mFavouredSkill);
            writer.Write(mFactReaction);
        }
    };
    
    public class FADTstruct
    {
        // Which attributes we like
        public int[] mAttribute { get; } = new int[2]; // 2

        public RankData[] mRankData { get; } = new RankData[10];

        public int[] mSkills { get; } = new int[7]; // IDs of skills this faction require
        // Each element will either contain an Skill index, or -1.
        
        public int mIsHidden { get; set; } // 1 - hidden from player

        public void Load(EsmReader reader)
        {
            reader.getSubHeader();
            for (var i = 0; i < mAttribute.Length; ++i)
                mAttribute[i] = reader.BinaryReader.ReadInt32();
            for (var i = 0; i < mRankData.Length; ++i)
            {
                RankData rankData = new();
                rankData.Load(reader);
                mRankData[i] = rankData;
            }

            for (var i = 0; i < mSkills.Length; ++i)
                mSkills[i] = reader.BinaryReader.ReadInt32();
            mIsHidden = reader.BinaryReader.ReadInt32();
            if (mIsHidden > 1)
                throw new Exception("Unknown flag!");
        }

        public void Save(EsmWriter writer)
        {
            writer.startSubRecord(RecordName.FADT);
            foreach (var attr in mAttribute)
                writer.Write(attr);
            foreach (var rank in mRankData)
                rank.Save(writer);
            foreach (var t in mSkills)
                writer.Write(t);
            writer.Write(mIsHidden);
            writer.endRecord(RecordName.FADT);
        }
    }; // 240 bytes
    
    public override RecordName Name => RecordName.FACT;
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }
    
    public string mName { get; set; }

    public Dictionary<RefId, int> mReactions = new();

    public FADTstruct mData { get; } = new();
    
    public string[] mRanks = new string[10];

    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        int rankCounter = 0;
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
                case RecordName.FNAM:
                    mName = reader.getHString();
                    break;
                case RecordName.RNAM:
                    if (rankCounter >= mRanks.Length)
                        throw new Exception("Rank out of range");

                    mRanks[rankCounter++] = reader.getHString();
                    break;
                case RecordName.FADT:
                    mData.Load(reader);
                    hasData = true;
                    break;
                case RecordName.ANAM:
                    var faction = reader.getRefId();
                    int reaction = 0;
                    reader.getHNT(RecordName.INTV, () => reaction = reader.BinaryReader.ReadInt32());

                    if (!mReactions.TryGetValue(faction, out var val) || val > reaction)
                        mReactions[faction] = reaction;
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
            throw new MissingSubrecordException(RecordName.FADT);
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.NAME, mId);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        writer.writeHNOCString(RecordName.FNAM, mName);

        foreach (var rank in mRanks)
        {
            if (string.IsNullOrEmpty(rank))
                break;

            writer.writeHNString(RecordName.RNAM, rank, 32);
        }

        mData.Save(writer);

        foreach (var item in mReactions)
        {
            writer.writeHNRefId(RecordName.ANAM, item.Key);
            writer.writeHNT(RecordName.INTV, item.Value);
        }
    }
}