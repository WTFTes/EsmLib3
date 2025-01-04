namespace EsmLib3.RefIds;

public class FormId : RefId
{
    public override RefIdType Type => RefIdType.FormId;

    public uint mIndex { get; set; } = 0;
    public int mContentFile { get; set; } = -1;

    public bool hasContentFile() => mContentFile >= 0;

    public bool isZeroOrUnset() => mIndex == 0 && (mContentFile == 0 || mContentFile == -1);

    public override bool Equals(RefId? other)
    {
        return other is { Type: RefIdType.FormId } and FormId formId &&
               formId.mIndex == mIndex && formId.mContentFile == mContentFile;
    }

    public override int GetHashCode()
    {
        return ((long)mIndex << 32 | mContentFile).GetHashCode();
    }

    public override string ToString()
    {
        ulong value = 0;
        string buf = "";
        if (mContentFile >= 0)
        {
            if ((mIndex & 0xff000000) != 0)
                throw new Exception($"Invalid FormId index value: 0x{mIndex:x}");
            value = mIndex | ((ulong)mContentFile << 24);
        }
        else
        {
            buf = "@";
            value = mIndex | ((ulong)(-mContentFile - 1) << 32);
        }

        return buf + $"0x{value:x}";
    }
}
