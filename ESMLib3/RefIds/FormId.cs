namespace EsmLib3.RefIds;

public struct FormId : IEquatable<FormId>
{
    public FormId()
    {
    }

    public uint Index { get; set; } = 0;
    public int ContentFile { get; set; } = -1;

    public bool HasContentFile => ContentFile >= 0;

    public bool IsZeroOrUnset => Index == 0 && (ContentFile == 0 || ContentFile == -1);

    public override int GetHashCode()
    {
        return ((ulong)Index << 32 | (ulong)ContentFile).GetHashCode();
    }

    public override string ToString()
    {
        ulong value = 0;
        string buf = "";
        if (ContentFile >= 0)
        {
            if ((Index & 0xff000000) != 0)
                throw new Exception($"Invalid FormId index value: 0x{Index:x}");
            value = Index | ((ulong)ContentFile << 24);
        }
        else
        {
            buf = "@";
            value = Index | ((ulong)(-ContentFile - 1) << 32);
        }

        return buf + $"0x{value:x}";
    }
    
    public string ToDebugString() => $"FormId:{ToString()}";

    public bool Equals(FormId other)
    {
        return Index == other.Index && ContentFile == other.ContentFile;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        return Equals((FormId)obj);
    }

    public static bool operator ==(FormId left, FormId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FormId left, FormId right)
    {
        return !(left == right);
    }
}
