using EsmLib3.Enums;
using EsmLib3.RefIds;

namespace EsmLib3;

public class EsmWriter : IDisposable
{
    public EsmEncoding mEncoding { get; set; } = EsmEncoding.English;
    
    private class RecordData
    {
        public RecordName Name;
        public long Position;
        public uint Size;
    }

    private Header mHeader;
    
    private List<RecordData> mRecords = new();

    private bool mCounting;
    private int mRecordCount;

    private BinaryWriter? BinaryWriter { get; set; }

    public FormatVersion getFormatVersion() => mHeader.FormatVersion;

    public EsmVersion getVersion() => mHeader.mData.version;

    public void save(BinaryWriter writer, EsmData data)
    {
        mRecordCount = 0;
        mRecords.Clear();
        mCounting = true;
        BinaryWriter = writer;

        startRecord(RecordName.TES3, RecordFlag.None);

        mHeader = data.Header;
        mHeader.mData.records = data.Records.Count;
        
        mHeader.Save(this);

        endRecord(RecordName.TES3);

        foreach (var record in data.Records)
        {
            startRecord(record.mType, record.mFlags);
            record.Save(this);
            endRecord(record.mType);
        }
    }

    public void startRecord(RecordName name, RecordFlag flags)
    {
        mRecordCount++;

        writeName(name);

        RecordData rec = new();
        rec.Name = name;
        rec.Position = BinaryWriter.BaseStream.Position;
        rec.Size = 0;
        Write((int)0); // Size goes here
        Write((int)0); // Unused header?
        Write((int)flags);
        mRecords.Add(rec);
    }

    public void startSubRecord(RecordName name)
    {
        // Sub-record hierarchies are not properly supported in ESMReader. This should be fixed later.
        if (mRecords.Count <= 1)
            throw new Exception("Sub-record hierarchies are not properly supported in ESMReader");

        writeName(name);
        RecordData rec = new();
        rec.Name = name;
        rec.Position = BinaryWriter.BaseStream.Position;
        rec.Size = 0;
        Write((int)0); // Size goes here
        mRecords.Add(rec);
    }

    public void endRecord(RecordName name)
    {
        var rec = mRecords[^1];
        if (rec.Name != name)
            throw new Exception("Record name mismatch");

        mRecords.RemoveAt(mRecords.Count - 1);

        BinaryWriter.BaseStream.Seek(rec.Position, SeekOrigin.Begin);

        mCounting = false;
        Write(rec.Size);
        mCounting = true;

        BinaryWriter.BaseStream.Seek(0, SeekOrigin.End);
    }
    
    public void writeHNString(RecordName name, string data)
    {
        startSubRecord(name);
        writeHString(data);
        endRecord(name);
    }
    
    public void writeHNString(RecordName name, string data, int size)
    {
        startSubRecord(name);
        writeHString(data, size);
        endRecord(name);
    }

    public void writeHNT(RecordName name, Action func)
    {
        startSubRecord(name);
        func.Invoke();
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, long data)
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, ulong data)
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, int data)
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, uint data)
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, short data)
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, ushort data)
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, byte data)
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, sbyte data) 
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, float data) 
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }
    
    public void writeHNT(RecordName name, byte[] data) 
    {
        startSubRecord(name);
        Write(data);
        endRecord(name);
    }

    public int writeHString(string data, int size = 0)
    {
        if (string.IsNullOrEmpty(data) && size == 0)
        {
            Write((byte)0);

            return 1;
        }
        else
        {
            var bytes = mEncoding.SystemEncoding.GetBytes(data);
            Write(bytes, size);

            return bytes.Length;
        }
    }

    public void writeHCString(string data)
    {
        writeHString(data);
        if (!string.IsNullOrEmpty(data))
            Write((byte)0);
    }

    public void writeHNCString(RecordName name, string data)
    {
        startSubRecord(name);
        writeHCString(data);
        endRecord(name);
    }

    public void writeMaybeFixedSizeString(string data, int size)
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxLimitedSizeStringsFormatVersion)
        {
            var bytes = mEncoding.SystemEncoding.GetBytes(data);
            Write(bytes, size);
        }
        else
            writeStringWithSize(data);
    }
    
    public void writeMaybeFixedSizeRefId(RefId value, int size)
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxStringRefIdFormatVersion)
        {
            writeMaybeFixedSizeString(value.GetRefIdString(), size);
            return;
        }

        value.Save(this, true);
    }

    public void writeStringWithSize(string data)
    {
        var bytes = mEncoding.SystemEncoding.GetBytes(data);
        if (bytes.LongLength > uint.MaxValue)
            throw new Exception(
                $"String size is too long: \"{data.Substring(0, 64)}<...>\" ({bytes.LongLength} > {uint.MaxValue})");
        Write(bytes.Length);
        Write(bytes);
    }

    private void writeName(RecordName name)
    {
        Write((int)name);
    }

    private void Count(uint size)
    {
        if (!mCounting)
            return;
        
        foreach (var record in mRecords)
            record.Size += size;
    }
    
    public void Write(long value)
    {
        Count(sizeof(long));
        BinaryWriter?.Write(value);
    }

    public void Write(ulong value)
    {
        Count(sizeof(ulong));
        BinaryWriter?.Write(value);
    }

    public void Write(int value)
    {
        Count(sizeof(int));
        BinaryWriter?.Write(value);
    }

    public void Write(uint value)
    {
        Count(sizeof(uint));
        BinaryWriter?.Write(value);
    }

    public void Write(short value)
    {
        Count(sizeof(short));
        BinaryWriter?.Write(value);
    }

    public void Write(ushort value)
    {
        Count(sizeof(ushort));
        BinaryWriter?.Write(value);
    }

    public void Write(byte value)
    {
        Count(sizeof(byte));
        BinaryWriter?.Write(value);
    }

    public void Write(sbyte value)
    {
        Count(sizeof(sbyte));
        BinaryWriter?.Write(value);
    }
    
    public void Write(float value)
    {
        Count(sizeof(float));
        BinaryWriter?.Write(value);
    }

    public void Write(byte[] value, int length = 0)
    {
        if (length > 0)
        {
            if (value.Length > length)
                throw new Exception("Value length is greater than the max allowed length");
            Count((uint)length);
            BinaryWriter.Write(value);
            if (length > value.Length)
                BinaryWriter.Write(new byte[length - value.Length]);
        }
        else
        {
            Count((uint)value.Length);
            BinaryWriter.Write(value);
        }
    }

    public void Dispose()
    {
        BinaryWriter?.Dispose();
    }

    public void writeHNCRefId(RecordName name, RefId value)
    {
        startSubRecord(name);
        writeHCRefId(value);
        endRecord(name);
    }
    
    public void writeHNRefId(RecordName name, RefId value)
    {
        startSubRecord(name);
        writeHRefId(value);
        endRecord(name);
    }
    
    public void writeHNRefId(RecordName name, RefId value, int size)
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxStringRefIdFormatVersion)
        {
            writeHNString(name, value.GetRefIdString(), size);
            return;
        }

        writeHNRefId(name, value);
    }

    private void writeHCRefId(RefId value)
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxStringRefIdFormatVersion)
        {
            writeHCString(value.GetRefIdString());
            return;
        }

        writeRefId(value);
    }
    
    private void writeHRefId(RefId value)
    {
        if (mHeader.FormatVersion <= FormatVersion.MaxStringRefIdFormatVersion)
        {
            writeHString(value.GetRefIdString());
            return;
        }
        
        writeRefId(value);
    }

    private void writeRefId(RefId value)
    {
        value.Save(this, false);
    }

    public void writeDeleted()
    {
        writeHNString(RecordName.DELE, "", 4);
    }

    public void writeHNOCString(RecordName name, string data)
    {
        if (!string.IsNullOrEmpty(data))
            writeHNCString(name, data);
    }
    
    public void writeHNOString(RecordName name, string data)
    {
        if (!string.IsNullOrEmpty(data))
            writeHNString(name, data);
    }

    public void writeHNOCRefId(RecordName name, RefId value)
    {
        if (!value.IsEmpty)
            writeHNCRefId(name, value);
    }

    public void writeHNORefId(RecordName name, RefId value)
    {
        if (!value.IsEmpty)
            writeHNRefId(name, value);
    }

    public void writeFormId(FormId formId, bool wide, RecordName tag = RecordName.FRMR)
    {
        if (wide)
            writeHNT(tag, () =>
            {
                Write(formId.Index);
                Write(formId.ContentFile);
            });
        else
            writeHNT(tag, formId.toUint32());
    }
}
