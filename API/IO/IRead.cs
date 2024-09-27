namespace GMutagen.IO;

public interface IRead<in TId, out TValue> where TId : notnull
{
    int Count { get; }
    TValue this[TId id] { get; }

    TValue Read(TId id);
    bool Contains(TId id);
}