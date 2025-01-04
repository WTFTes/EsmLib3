namespace EsmLib3.RefIds;

public class IndexRefId : RefId
{
    public override RefIdType Type => RefIdType.Index;

    public RecordName RecordType { get; set; }
    
    public uint Value { get; set; }

    public override bool Equals(RefId? other)
    {
        return other is { Type: RefIdType.Index } and IndexRefId refId &&
               refId.RecordType == RecordType && refId.Value == Value;
    }

    public override int GetHashCode()
    {
        return ((long)RecordType << 32 | Value).GetHashCode();
    }

    public override string ToString()
    {
        return $"{RecordType.ToMagic()}:0x{Value:x}";
    }
}
