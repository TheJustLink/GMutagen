namespace GMutagen.v8.IO;

public interface IReadWrite<in TId, TValue> : IRead<TId, TValue>, IWrite<TId, TValue>
{
    TValue this[TId id] { get; set; }
}
public interface IReadWrite<in TId> : IRead<TId>, IWrite<TId> { }