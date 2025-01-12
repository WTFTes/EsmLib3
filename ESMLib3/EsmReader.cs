using System.Diagnostics;
using EsmLib3.Enums;
using EsmLib3.Records;
using EsmLib3.RefIds;

namespace EsmLib3;

public class EsmReader : IDisposable
{
    public BinaryReader? BinaryReader { get; private set; }

    private Context mCtx;

    private RecordFlag mRecordFlags;

    private Header mHeader = new();
    
    public EsmEncoding mEncoding { get; set; } = EsmEncoding.English;

    private Dictionary<int, int>? _mContentFileMapping = null;

    private long mFileSize;
    
    public long getFileSize() => mFileSize;

    public EsmReader()
    {
        clearCtx();
        mCtx.index = 0;
    }

    public void RestoreContext(Context rc)
    {
        if (mCtx.FileName != rc.FileName)
            openRaw(rc.FileName);

        mCtx = rc;

        BinaryReader!.BaseStream.Seek(mCtx.filePos, SeekOrigin.Begin);
    }

    public void open(BinaryReader reader, string name)
    {
        openRaw(reader, name);

        if (GetRecordName() != RecordName.TES3)
            throw new Exception("Not a valid Morrowind file");

        GetRecHeader();

        mHeader.Load(this);
    }

    public void open(string name)
    {
        var reader = new BinaryReader(File.OpenRead(name));
        open(reader, name);
    }

    private void openRaw(string name)
    {
        var reader = new BinaryReader(File.OpenRead(name));
        openRaw(reader, name);
    }

    private void openRaw(BinaryReader reader, string name)
    {
        close();
        BinaryReader = reader;
        mCtx.FileName = name;
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        mCtx.leftFile = mFileSize = reader.BaseStream.Length;
    }

    private void close()
    {
        clearCtx();
        BinaryReader = null;
        mHeader.blank();
    }

    private void clearCtx()
    {
        mCtx.FileName = "";
        mCtx.leftFile = 0;
        mCtx.leftRec = 0;
        mCtx.leftSub = 0;
        mCtx.subCached = false;
        mCtx.RecName = 0;
        mCtx.SubName = 0;
    }

    public EsmData Read()
    {
        return Read(new ReadSettings());
    }

    public EsmData Read(ReadSettings settings)
    {
        if (BinaryReader == null)
            throw new Exception("Not open");

        EsmData data = new()
        {
            Encoding = mEncoding,
        };

        while (HasMoreRecs)
        {
            var n = GetRecordName();

            GetRecHeader();

            var canRaw = false;
            RecordBase record;
            if (settings.RecordsToGet == null)
            {
                record = RecordBase.Create(n);
                if (record == null)
                {
                    if (settings.SkipUnknownRecords)
                    {
                        SkipRecord();
                        continue;
                    }

                    if (settings.UnknownRecordsToRaw)
                        record = MakeRawRecord(n);
                    else
                        throw new Exception($"Unknown record {n.ToMagic()}");
                }
            }
            else if (settings.RecordsToGet.Contains(n))
            {
                record = RecordBase.Create(n);
                if (record == null)
                    throw new Exception($"Unknown record {n.ToMagic()}");
            }
            else if (settings.SkippedRecordsToRaw)
                record = MakeRawRecord(n);
            else
            {
                SkipRecord();
                continue;
            }

            record.mFlags = mRecordFlags;

            record.Load(this);

            data.AddRecord(record);
        }

        data.Header = mHeader;

        return data;
    }

    private RecordBase MakeRawRecord(RecordName name)
    {
        return new TypedRecord<RawRecord>()
        {
            Data = { RawName = name },
            mType = name
        };
    }

    private RecordName GetRecordName()
    {
        if (!HasMoreRecs)
            throw new Exception("No more records");
        if (HasMoreSubs)
            throw new Exception("Previous record contains unread bytes");
        
        if (mCtx.leftRec < 0)
        {
            Debug.WriteLine("We went out of the previous record's bounds. Backtrack.");
            BinaryReader.BaseStream.Seek(mCtx.leftRec, SeekOrigin.Current);
        }

        mCtx.RecName = (RecordName)BinaryReader!.ReadUInt32();
        mCtx.leftFile -= sizeof(uint);
        
        // Make sure we don't carry over any old cached subrecord
        // names. This can happen in some cases when we skip parts of a
        // record.
        mCtx.subCached = false;

        return mCtx.RecName;
    }

    private void GetRecHeader()
    {
        if (mCtx.leftFile < 3 * sizeof(uint))
            throw new Exception("End of file while reading record header");

        if (mCtx.leftRec > 0)
            throw new Exception("Previous record contains unread bytes");

        mCtx.leftRec = BinaryReader!.ReadUInt32();
        BinaryReader.ReadUInt32(); // This header entry is always zero
        mRecordFlags = (RecordFlag)BinaryReader.ReadUInt32();
        mCtx.leftFile -= 3 * sizeof(uint);

        // Check that sizes add up
        if (mCtx.leftFile < mCtx.leftRec)
            ReportSubSizeMismatch(mCtx.leftFile, mCtx.leftRec);

        // Adjust number of bytes mCtx.left in file
        mCtx.leftFile -= mCtx.leftRec;
    }

    public void SkipRecord()
    {
        skip((uint)mCtx.leftRec);
        mCtx.leftRec = 0;
        mCtx.subCached = false;
    }

    public bool HasMoreRecs => mCtx.leftFile > 0;
    public bool HasMoreSubs => mCtx.leftRec > 0;

    public string FileName => mCtx.FileName;

    private void ReportSubSizeMismatch(long want, long got)
    {
        throw new Exception($"Record size mismatch, requested {want}, got {got}");
    }

    public bool IsNextSub(RecordName name)
    {
        if (!HasMoreSubs)
            return false;

        GetSubName();

        mCtx.subCached = mCtx.SubName != name;

        return !mCtx.subCached;
    }

    public void GetSubName()
    {
        if (mCtx.subCached)
        {
            mCtx.subCached = false;
            return;
        }

        mCtx.SubName = (RecordName)BinaryReader!.ReadUInt32();
        mCtx.leftRec -= sizeof(uint);
    }

    public void getSubNameIs(RecordName name)
    {
        GetSubName();
        if (mCtx.SubName != name)
            throw new Exception($"Expected subrecord {name.ToMagic()} but got {mCtx.SubName.ToMagic()}");
    }

    public bool getHNOT(RecordName name, ref uint value)
    {
        uint val = 0;
        var res = getHNOT(name, () => { val = BinaryReader!.ReadUInt32(); });

        value = val;

        return res;
    }

    public bool getHNOT(RecordName name, Action action)
    {
        if (IsNextSub(name))
        {
            getHT(action);
            return true;
        }

        return false;
    }

    public void getHT(Action action)
    {
        // constexpr size_t size = (0 + ... + sizeof(Args));
        getSubHeader();
        // if (mCtx.leftSub != size)
        //     reportSubSizeMismatch(size, mCtx.leftSub);
        action();
    }

    public void getHNT(RecordName name, Action action)
    {
        // constexpr size_t size = (0 + ... + sizeof(Args));
        getSubNameIs(name);
        getSubHeader();
        // if (mCtx.leftSub != size)
        //     reportSubSizeMismatch(size, mCtx.leftSub);
        action();
    }

    public void getSubHeader()
    {
        if (mCtx.leftRec < sizeof(uint))
            throw new Exception($"End of record while reading sub-record header: {mCtx.leftRec} < 4");

        mCtx.leftSub = BinaryReader!.ReadUInt32();
        mCtx.leftRec -= sizeof(uint);

        mCtx.leftRec -= mCtx.leftSub;
    }

    public string getMaybeFixedStringSize(uint size)
    {
        if (mHeader.FormatVersion > FormatVersion.MaxLimitedSizeStringsFormatVersion)
        {
            size = BinaryReader!.ReadUInt32();
            if (size > mCtx.leftSub)
                throw new Exception($"String does not fit subrecord ({size} > {mCtx.leftSub})");
        }

        return getString(size);
    }

    public string getString(uint size)
    {
        var bytes = BinaryReader!.ReadBytes((int)size).TakeWhile(b => b != 0);

        return mEncoding.SystemEncoding.GetString(bytes.ToArray());
    }

    public string getHString()
    {
        getSubHeader();
        if (mHeader.FormatVersion > FormatVersion.MaxStringRefIdFormatVersion)
            return getString(mCtx.leftSub);

        if (mCtx.leftSub == 0 && HasMoreSubs && BinaryReader!.PeekChar() == 0)
        {
            --mCtx.leftRec;
            BinaryReader.ReadChar();
            return "";
        }

        return getString(mCtx.leftSub);
    }

    public string getHNString(RecordName name)
    {
        getSubNameIs(name);
        return getHString();
    }

    public RefId getHNRefId(RecordName name)
    {
        getSubNameIs(name);
        return getRefId();
    }

    public RefId getRefId(uint size)
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxStringRefIdFormatVersion)
            return RefId.StringRefId(getString(size));
        return getRefIdImpl(size);
    }

    public RefId getRefId()
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxStringRefIdFormatVersion)
            return RefId.StringRefId(getHString());
        getSubHeader();
        return getRefIdImpl(mCtx.leftSub);
    }

    private RefId getRefIdImpl(uint size)
    {
         var type = (RefIdType)BinaryReader!.ReadByte();

         switch (type)
         {
             case RefIdType.Empty:
                 return new RefId();
             case RefIdType.SizedString:
             {
                 var minSize = 1 + 4;
                 if (size < minSize)
                     throw new Exception($"Requested RefId record size is too small ({size} < {minSize}");

                 var storedSize = BinaryReader.ReadUInt32();
                 var maxSize = size - minSize;
                 if (storedSize > maxSize)
                     throw new Exception($"RefId string does not fit subrecord size ({storedSize} > {maxSize})");

                 return RefId.StringRefId(getString(storedSize));
             }
             case RefIdType.UnsizedString:
             {
                 if (size < 1)
                     throw new Exception($"Requested RefId record size is too small ({size} < 1)");

                 return RefId.StringRefId(getString((uint)(size - 1)));
             }
             case RefIdType.FormId:
             {
                 FormId formId = new();
                 formId.Index = BinaryReader.ReadUInt32();
                 formId.ContentFile = BinaryReader.ReadInt32();
                 if (applyContentFileMapping(formId))
                 {
                     if (formId.IsZeroOrUnset)
                         return new RefId();
                     if (formId.HasContentFile)
                         return RefId.FormIdRefId(formId);
                     throw new Exception("RefId can't be a generated FormId");
                 }

                 return new RefId();
             }
             case RefIdType.Generated:
             {
                 var value = BinaryReader.ReadUInt64();
                 return RefId.Generated(value);
             }
             case RefIdType.Index:
             {
                 var recordType = (RecordName)BinaryReader.ReadInt32();
                 var value = BinaryReader.ReadUInt32();

                 return RefId.Index(recordType, value);
             }
             case RefIdType.Esm3ExteriorCell:
             {
                 var x = BinaryReader.ReadInt32();
                 var y = BinaryReader.ReadInt32();

                 return RefId.Esm3ExteriorCell(x, y);
             }
         }
         
         throw new Exception($"Unsupported RefIdType: {type}");
    }

    public RefId getHNORefId(RecordName name)
    {
        if (IsNextSub(name))
            return getRefId();

        return new RefId();
    }

    private bool applyContentFileMapping(FormId formId)
    {
        if (_mContentFileMapping != null && formId.HasContentFile)
        {
            if (!_mContentFileMapping.TryGetValue(formId.ContentFile, out var mapping))
                return false;

            formId.ContentFile = mapping;
        }

        return true;
    }

    public void Dispose()
    {
        BinaryReader?.Dispose();
    }

    public uint GetSubSize() => mCtx.leftSub;
    
    public RecordName retSubName() => mCtx.SubName;

    public RecordFlag getRecordFlags() => mRecordFlags;

    public void skipHSub()
    {
        getSubHeader();
        skip(mCtx.leftSub);
    }

    public void skip(uint size)
    {
        BinaryReader!.BaseStream.Seek(size, SeekOrigin.Current);
    }

    public FormatVersion getFormatVersion() => mHeader.FormatVersion;

    public RefId getMaybeFixedRefIdSize(uint size)
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxStringRefIdFormatVersion)
            return RefId.StringRefId(getMaybeFixedStringSize(size));

        return getRefIdImpl(mCtx.leftSub);
    }

    public void cacheSubName() => mCtx.subCached = true;

    public Context getContext()
    {
        mCtx.filePos = BinaryReader!.BaseStream.Position;
        return mCtx;
    }

    public byte[] GetRawRecord()
    {
        var leftRec = mCtx.leftRec;

        mCtx.leftRec = 0;
        mCtx.leftSub = 0;
        mCtx.subCached = false;

        return BinaryReader.ReadBytes((int)leftRec);
    }

    public List<int> getParentFileIndices() => mCtx.parentFileIndices;

    public int getIndex() => mCtx.index;
}