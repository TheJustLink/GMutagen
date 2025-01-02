namespace GMutagen.v9.IO;

public interface IReadWrite<in TId, TValue>
    : IRead<TId, TValue>, IWrite<TId, TValue>
    where TId : notnull
{
    new TValue this[TId id] { get; set; }
}