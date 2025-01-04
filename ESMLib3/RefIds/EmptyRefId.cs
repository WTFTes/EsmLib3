namespace EsmLib3.RefIds;

public class EmptyRefId : RefId
{
    public override RefIdType Type => RefIdType.Empty;

    public override bool Equals(RefId? other)
    {
        return other != null && other.Type == RefIdType.Empty;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override string ToString() => "";
}