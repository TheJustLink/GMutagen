namespace GMutagen.v9.IO;

public interface IReadWrite<in TId, TValue>
    : IRead<TId, TValue>, IWrite<TId, TValue>, IReadWrite
    where TId : notnull
{
    new TValue this[TId id] { get; set; }
}

public interface IReadWrite
{
}