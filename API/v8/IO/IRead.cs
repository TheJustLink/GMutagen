namespace GMutagen.v8.IO;

public interface IRead<in TId, out TValue> where TId : notnull
{
    TValue this[TId id] { get; }
    TValue Read(TId id);
    bool Contains(TId id);
}