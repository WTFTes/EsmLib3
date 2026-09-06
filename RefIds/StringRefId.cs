using System.Globalization;

namespace EsmLib3.RefIds;

public struct StringRefId : IEquatable<StringRefId>
{
    public string Value { get; set; } = "";

    public StringRefId()
    {
    }

    public StringRefId(string value) => Value = value;

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public override string ToString() => Value;

    private static UnicodeCategory[] _nonRenderingCategories =
    [
        UnicodeCategory.Control,
        UnicodeCategory.OtherNotAssigned,
        UnicodeCategory.Surrogate
    ];
    
    public string ToDebugString() => "\"" + string.Join("", Value.Select(_=> _nonRenderingCategories.Contains(char.GetUnicodeCategory(_)) ? $"\\x{_:X}" : _.ToString())) + "\"";

    public bool Equals(StringRefId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is StringRefId other && Equals(other);
    }

    public static bool operator ==(StringRefId left, StringRefId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StringRefId left, StringRefId right)
    {
        return !left.Equals(right);
    }
}
