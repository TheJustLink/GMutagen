namespace GMutagen.v6;

public interface IRead<in TId, out TValue>
{
    TValue this[TId id] { get; }
    TValue Read(TId id);
}