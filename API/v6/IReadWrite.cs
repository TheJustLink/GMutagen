namespace GMutagen.v6;

public interface IReadWrite<in TId, TValue> : IRead<TId, TValue>, IWrite<TId, TValue>
{
    TValue this[TId id] { get; set; }
}