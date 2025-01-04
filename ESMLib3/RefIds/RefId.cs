namespace EsmLib3.RefIds;

public abstract class RefId
{
    public abstract RefIdType Type { get; }

    public abstract bool Equals(RefId? other);

    public abstract override int GetHashCode();
}
