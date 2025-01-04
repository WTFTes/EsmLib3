namespace EsmLib3.RefIds;

public class GeneratedRefId : RefId
{
    public override RefIdType Type => RefIdType.Generated;
    
    public ulong Value { get; set; }
    
    public override bool Equals(RefId? other)
    {
        return other is { Type: RefIdType.Generated } and GeneratedRefId refId &&
               refId.Value == Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => $"0x{Value:x}";
}