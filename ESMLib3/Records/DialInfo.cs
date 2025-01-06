using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records;

public class DialInfo : AbstractRecord
{
    public enum Gender
    {
        Male = 0,
        Female = 1,
        NA = -1
    }
    
    // Journal quest indices (introduced with the quest system in Tribunal)
    public enum QuestStatus
    {
        QS_None = 0,
        QS_Name = 1,
        QS_Finished = 2,
        QS_Restart = 3
    }

    public class DATAstruct
    {
        public int mUnknown1 { get; set; }

        // Used for dialogue responses
        // Used for journal entries
        public int mDispositionOrJournalIndex { get; set; }
        
        public sbyte mRank { get; set; } = -1; // Rank of NPC
        
        public Gender mGender { get; set; } = Gender.NA;    // sbyte
        
        public sbyte mPCrank { get; set; } = -1; // Player rank
        
        public sbyte mUnknown2 { get; set; }
    }
    
    // The rules for whether or not we will select this dialog item.
    public class SelectStruct
    {
        public string mSelectRule { get; set; } // This has a complicated format
        public Variant mValue { get; set; } = new();
    }

    public override RecordName Name => RecordName.INFO;
    
    // Id of this, previous and next INFO items
    public RefId mId { get; set; }
    
    public RefId mPrev { get; set; }
    
    public RefId mNext { get; set; }

    // Various references used in determining when to select this item.
    public RefId mActor { get; set; }
    
    public RefId mRace { get; set; }
    
    public RefId mClass { get; set; }
    
    public RefId mFaction { get; set; }
    
    public RefId mPcFaction { get; set; }
    
    public RefId mCell { get; set; }
    
    // Rules for when to include this item in the final list of options
    // visible to the player.
    public List<SelectStruct> mSelects = new();
    
    // Sound and text associated with this item
    public string mSound { get; set; }
    
    public string mResponse { get; set; }
    
    // Result script (uncompiled) to run whenever this dialog item is
    // selected
    public string mResultScript { get; set; }

    // ONLY include this item the NPC is not part of any faction.
    public bool mFactionLess { get; set; }

    // Status of this quest item
    public QuestStatus mQuestStatus { get; set; }

    public DATAstruct mData { get; set; } = new();
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        mId = reader.getHNRefId(RecordName.INAM);

        isDeleted = false;

        mQuestStatus = QuestStatus.QS_None;
        mFactionLess = false;
        
        mPrev = reader.getHNRefId(RecordName.PNAM);
        mNext = reader.getHNRefId(RecordName.NNAM);

        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.DATA:
                    reader.getHT(() =>
                    {
                        mData.mUnknown1 = reader.BinaryReader.ReadInt32();
                        mData.mDispositionOrJournalIndex = reader.BinaryReader.ReadInt32();
                        mData.mRank = reader.BinaryReader.ReadSByte();
                        mData.mGender = (Gender)reader.BinaryReader.ReadSByte();
                        mData.mPCrank = reader.BinaryReader.ReadSByte();
                        mData.mUnknown2 = reader.BinaryReader.ReadSByte();
                    });
                    break;
                case RecordName.ONAM:
                    mActor = reader.getRefId();
                    break;
                case RecordName.RNAM:
                    mRace = reader.getRefId();
                    break;
                case RecordName.CNAM:
                    mClass = reader.getRefId();
                    break;
                case RecordName.FNAM:
                    mFaction = reader.getRefId();
                    if (mFaction == "FFFF")
                        mFactionLess = true;
                    break;
                case RecordName.ANAM:
                    mCell = reader.getRefId();
                    break;
                case RecordName.DNAM:
                    mPcFaction = reader.getRefId();
                    break;
                case RecordName.SNAM:
                    mSound = reader.getHString();
                    break;
                case RecordName.NAME:
                    mResponse = reader.getHString();
                    break;
                case RecordName.SCVR:
                    SelectStruct ss = new();
                    ss.mSelectRule = reader.getHString();
                    ss.mValue.Read(reader, Variant.Format.Info);
                    mSelects.Add(ss);
                    break;
                case RecordName.BNAM:
                    mResultScript = reader.getHString();
                    break;
                case RecordName.QSTN:
                    mQuestStatus = QuestStatus.QS_Name;
                    reader.SkipRecord();
                    break;
                case RecordName.QSTF:
                    mQuestStatus = QuestStatus.QS_Finished;
                    reader.SkipRecord();
                    break;
                case RecordName.QSTR:
                    mQuestStatus = QuestStatus.QS_Restart;
                    reader.SkipRecord();
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                default:
                    throw new UnknownSubrecordException(reader.retSubName());
            }
        }

        return;
    }

    public override void Save(EsmWriter writer, bool isDeleted)
    {
        writer.writeHNCRefId(RecordName.INAM, mId);
        writer.writeHNCRefId(RecordName.PNAM, mPrev);
        writer.writeHNCRefId(RecordName.NNAM, mNext);

        if (isDeleted)
        {
            writer.writeDeleted();
            return;
        }

        writer.writeHNT(RecordName.DATA, () =>
        {
            writer.Write(mData.mUnknown1);
            writer.Write(mData.mDispositionOrJournalIndex);
            writer.Write(mData.mRank);
            writer.Write((sbyte)mData.mGender);
            writer.Write(mData.mPCrank);
            writer.Write(mData.mUnknown2);
        });
        writer.writeHNOCRefId(RecordName.ONAM, mActor);
        writer.writeHNOCRefId(RecordName.RNAM, mRace);
        writer.writeHNOCRefId(RecordName.CNAM, mClass);
        writer.writeHNOCRefId(RecordName.FNAM, mFaction);
        writer.writeHNOCRefId(RecordName.ANAM, mCell);
        writer.writeHNOCRefId(RecordName.DNAM, mPcFaction);
        writer.writeHNOCString(RecordName.SNAM, mSound);
        writer.writeHNOString(RecordName.NAME, mResponse);

        foreach (var item in mSelects)
        {
            writer.writeHNString(RecordName.SCVR, item.mSelectRule);
            item.mValue.Write(writer, Variant.Format.Info);
        }

        writer.writeHNOString(RecordName.BNAM, mResultScript);

        switch (mQuestStatus)
        {
            case QuestStatus.QS_Name:
                writer.writeHNT(RecordName.QSTN, (byte)1);
                break;
            case QuestStatus.QS_Finished:
                writer.writeHNT(RecordName.QSTF, (byte)1);
                break;
            case QuestStatus.QS_Restart:
                writer.writeHNT(RecordName.QSTR, (byte)1);
                break;
        }
    }
}