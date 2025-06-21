using Identity.Interfaces;
using Identity.Realizations;

namespace Identity.Compose;

public class Identity : IId, IEquatable<Identity>
{
    private readonly HashSet<IId> _ids = new();
    
    public Identity(HashSet<IId> ids)
    {
        _ids = ids;
    }
    public Identity(params IId[] ids)
    {
        foreach (var id in ids)
        {
            _ids.Add(id);
        }
    }

    public void Add<T>(Id<T> id)
    {
        _ids.Add(id);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Identity);
    }

    public bool Equals(Identity other)
    {
        if (other == null) return false;
        return _ids.SetEquals(other._ids);
    }

    public bool Equals(IId id)
    {
        return Equals(id as Identity);
    }

    public override int GetHashCode()
    {
        return _ids.Aggregate(0, (hash, id) => hash ^ id.GetHashCode());
    }

    public object IdValue {
        get
        {
            if (_ids.Count != 1)
                return null;

            return _ids.First().IdValue;
        }
    }

    public override string ToString()
    {
        var result = string.Join(' ', _ids.Select(i => i.ToString()));
        return result;
    }

    public static Identity operator +(Identity id1, Identity id2)
    {
        var ides = new HashSet<IId>();

        foreach (var id in id1._ids)
            ides.Add(id);
        
        foreach (var id in id2._ids)
            ides.Add(id);
        
        return new Identity(ides);
    }

    public static implicit operator Identity(string id)
    {
        return new Identity(new Id<string>(id));
    }
    
    public static implicit operator Identity(long id)
    {
        return new Identity(new Id<long>(id));
    }

    public T To<T>()
    {
        if (_ids.Count != 1)
            return default;

        var id = _ids.First();
        return (T)id.IdValue;
    }
}