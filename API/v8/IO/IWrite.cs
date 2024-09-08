namespace GMutagen.v8.IO;

public interface IWrite<in TId, in TValue>
{
    TValue this[TId id] { set; }
    void Write(TId id, TValue value);
}
public interface IWrite<in TId>
{
    void Write<TValue>(TId id, TValue value);
}