namespace EsmLib3.RefIds;

public struct IndexRefId : IEquatable<IndexRefId>
{
    public IndexRefId()
    {
    }

    public IndexRefId(RecordName type, uint value)
    {
        RecordType = type;
        Value = value;
    }

    public RecordName RecordType { get; set; }

    public uint Value { get; set; }

    public override int GetHashCode()
    {
        return ((ulong)RecordType << 32 | Value).GetHashCode();
    }

    public override string ToString() => $"{RecordType.ToMagic()}:0x{Value:x}";

    public string ToDebugString() => $"Index:{ToString()}";

    public bool Equals(IndexRefId other)
    {
        return RecordType == other.RecordType && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is IndexRefId other && Equals(other);
    }

    public static bool operator ==(IndexRefId left, IndexRefId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IndexRefId left, IndexRefId right)
    {
        return !left.Equals(right);
    }
}
