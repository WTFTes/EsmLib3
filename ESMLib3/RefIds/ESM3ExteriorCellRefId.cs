namespace EsmLib3.RefIds;

public class ESM3ExteriorCellRefId : RefId
{
    public override RefIdType Type => RefIdType.Esm3ExteriorCell;

    public int mX { get; set; }
    public int mY { get; set; }

    public override bool Equals(RefId? other)
    {
        return other is { Type: RefIdType.Esm3ExteriorCell } and ESM3ExteriorCellRefId refId &&
               mX == refId.mX && mY == refId.mY;
    }

    public override int GetHashCode()
    {
        return ((long)mX << 32 | mY).GetHashCode();
    }

    public override string ToString() => $"#{mX} {mY}";
}
