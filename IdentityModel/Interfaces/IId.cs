namespace Identity.Interfaces;

public interface IId
{
    bool Equals(object? obj);
    bool Equals(IId? id);
    int GetHashCode();
    object IdValue { get; }
}

public interface ISingleId
{
    bool Equals(ISingleId? singleId);
}

public interface ISingleId<T> : ISingleId
{
    T Value { get; }
    
    bool Equals(ISingleId<T>? singleId);
}