using System.Runtime.InteropServices;
using EsmLib3.Enums;
using EsmLib3.Exceptions;
using EsmLib3.RefIds;

namespace EsmLib3.Records.Abstraction;

public abstract class LevelledListBase : AbstractRecord
{
    public class LevelItem
    {
        public RefId mId { get; set; }
        public ushort mLevel { get; set; }
    }

    public abstract RecordName sRecName { get; }
    
    public RefId mId { get; set; }
    
    public RecordFlag mRecordFlags { get; set; }

    protected int _flags { get; set; }
    
    public byte mChanceNone { get; set; } // Chance that none are selected (0-100)
    
    public List<LevelItem> mList { get; set; } = new();
    
    public override void Load(EsmReader reader, out bool isDeleted)
    {
        isDeleted = false;
        mRecordFlags = reader.getRecordFlags();

        var hasName = false;
        var hasList = false;
        while (reader.HasMoreSubs)
        {
            reader.GetSubName();
            switch (reader.retSubName())
            {
                case RecordName.NAME:
                    mId = reader.getRefId();
                    hasName = true;
                    break;
                case RecordName.DATA:
                    reader.getHT(() => _flags = reader.BinaryReader.ReadInt32());
                    break;
                case RecordName.NNAM:
                    reader.getHT(() => mChanceNone = reader.BinaryReader.ReadByte());
                    break;
                case RecordName.INDX:
                    reader.getHT(() => CollectionsMarshal.SetCount(mList, reader.BinaryReader.ReadInt32()));

                    // If this levelled list was already loaded by a previous content file,
                    // we overwrite the list. Merging lists should probably be left to external tools,
                    // with the limited amount of information there is in the records, all merging methods
                    // will be flawed in some way. For a proper fix the ESM format would have to be changed
                    // to actually track list changes instead of including the whole list for every file
                    // that does something with that list.
                    for (var i = 0; i < mList.Count; ++i)
                    {
                        LevelItem li = new();
                        li.mId = reader.getHNRefId(sRecName);
                        reader.getHNT(RecordName.INTV, () => li.mLevel = reader.BinaryReader.ReadUInt16());

                        mList[i] = li;
                    }

                    hasList = true;
                    break;
                case RecordName.DELE:
                    reader.skipHSub();
                    isDeleted = true;
                    break;
                default:
                    if (!hasList)
                    {
                        // Original engine ignores rest of the record, even if there are items following
                        mList.Clear();
                        reader.SkipRecord();
                    }
                    else
                        throw new UnknownSubrecordException(reader.retSubName());
                    break;
            }
        }
        
        if (!hasName)
            throw new MissingSubrecordException(RecordName.NAME);
    }

    public override void Save(EsmWriter reader, bool isDeleted)
    {
        throw new NotImplementedException();
    }
}