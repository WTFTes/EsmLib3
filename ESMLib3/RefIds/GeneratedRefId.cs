namespace EsmLib3.RefIds;

public struct GeneratedRefId : IEquatable<GeneratedRefId>
{
    public GeneratedRefId()
    {
    }
    
    public GeneratedRefId(ulong value)
    {
        Value = value;
    }

    public ulong Value { get; set; }
    
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => $"0x{Value:x}";
    
    public string ToDebugString() => $"Generated:{ToString()}";

    public bool Equals(GeneratedRefId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is GeneratedRefId other && Equals(other);
    }

    public static bool operator ==(GeneratedRefId left, GeneratedRefId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GeneratedRefId left, GeneratedRefId right)
    {
        return !left.Equals(right);
    }
}