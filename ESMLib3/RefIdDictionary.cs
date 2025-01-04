using EsmLib3.RefIds;

namespace EsmLib3;

public class RefIdDictionary<T>() : Dictionary<RefId, T?>(new RefIdEqualityComparer())
{
    class RefIdEqualityComparer : IEqualityComparer<RefId>
    {
        public bool Equals(RefId? x, RefId? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.Type != y.Type) return false;

            return x.Equals(y);
        }

        public int GetHashCode(RefId obj)
        {
            return obj.GetHashCode();
        }
    }
}
