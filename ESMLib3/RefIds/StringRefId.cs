namespace EsmLib3.RefIds;

public class StringRefId : RefId
{
    public override RefIdType Type => RefIdType.SizedString;

    public string Value { get; set; } = "";

    public StringRefId()
    {
    }

    public StringRefId(string value) => Value = value;

    public override bool Equals(RefId? other)
    {
        return other is { Type: RefIdType.SizedString } and StringRefId refId &&
               refId.Value == Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public override string ToString() => Value;
}
