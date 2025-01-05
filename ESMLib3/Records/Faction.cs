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
            throw new Exception("Not implemented yet");
        }
    };
    
    public class FADTstruct
    {
        // Which attributes we like
        public readonly int[] mAttribute = new int[2]; // 2

        public readonly RankData[] mRankData = new RankData[10];

        public readonly int[] mSkills = new int[7]; // IDs of skills this faction require
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
            throw new Exception("Not implemented yet");
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

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}