namespace GMutagen.IO;

public interface IReadWrite<in TId, TValue>
    : IRead<TId, TValue>, IWrite<TId, TValue>
    where TId : notnull
{
    TValue this[TId id] { get; set; } // ?
}