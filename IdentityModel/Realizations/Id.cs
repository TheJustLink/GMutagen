using Identity.Interfaces;

namespace Identity.Realizations;

public class Id<T>(T value) : ISingleId<T>, IId, IEquatable<Id<T>>
{
    public T Value { get; } = value;

    public bool Equals(ISingleId<T>? singleId)
    {
        return singleId != null && singleId.Value != null && singleId.Value.Equals(value);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Id<T>);
    }

    public bool Equals(Id<T>? other)
    {
        return other != null && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public bool Equals(IId? id)
    {
        return Equals(id as Id<T>);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Value);
    }

    public object IdValue => Value;

    public override string? ToString()
    {
        return Value?.ToString();
    }

    public bool Equals(ISingleId? singleId)
    {
        if (singleId is ISingleId<T> singleIdT)
            return Equals(singleIdT);
        
        return false;
    }

    public static implicit operator Id<T>(T data)
    {
        return new Id<T>(data);
    }
}