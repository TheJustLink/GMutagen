namespace GMutagen.IO;

public interface IWrite<in TId, in TValue> where TId : notnull
{
    TValue this[TId id] { set; }
    void Write(TId id, TValue value);
}