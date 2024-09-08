namespace GMutagen.v8.IO;

public interface IRead<in TId, out TValue>
{
    TValue this[TId id] { get; }
    TValue Read(TId id);
}
public interface IRead<in TId>
{
    TValue Read<TValue>(TId id);
}