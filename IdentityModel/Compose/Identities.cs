using Identity.Interfaces;

namespace Identity.Compose;

public class Identities(HashSet<IId> identities) : IId, IEquatable<Identities>
{
    public HashSet<IId> _identities = identities;
    public bool Equals(IId id)
    {
        var areEquals = _identities.Contains(id);
        return areEquals;
    }

    public object IdValue {
        get
        {
            if (_identities.Count != 1)
                return null;

            return _identities.First().IdValue;
        }
    }
    public bool Equals(Identities? other)
    {
        return other != null && _identities.Contains(other);
    }
}