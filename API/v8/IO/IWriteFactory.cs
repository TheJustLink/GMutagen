namespace GMutagen.v8.IO;

public interface IWriteFactory
{
    IWrite<TId, TValue> CreateWrite<TId, TValue>() where TId : notnull;
}