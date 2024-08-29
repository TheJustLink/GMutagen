namespace GMutagen.v6.IO;

public interface IWrite<in TId, in TValue>
{
    TValue this[TId id] { set; }
    void Write(TId id, TValue value);
}