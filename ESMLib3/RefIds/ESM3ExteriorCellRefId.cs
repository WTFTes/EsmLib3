namespace EsmLib3.RefIds;

public struct Esm3ExteriorCellRefId : IEquatable<Esm3ExteriorCellRefId>
{
    public Esm3ExteriorCellRefId()
    {
    }
    
    public Esm3ExteriorCellRefId(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    
    public int Y { get; set; }

    public override int GetHashCode()
    {
        return ((long)X << 32 | Y).GetHashCode();
    }

    public override string ToString() => $"#{X} {Y}";

    public string ToDebugString() => $"Esm3ExteriorCell:{X}:{Y}";
    
    public static bool operator ==(Esm3ExteriorCellRefId left, Esm3ExteriorCellRefId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Esm3ExteriorCellRefId left, Esm3ExteriorCellRefId right)
    {
        return !left.Equals(right);
    }

    public bool Equals(Esm3ExteriorCellRefId other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        return Equals((Esm3ExteriorCellRefId)obj);
    }
}
