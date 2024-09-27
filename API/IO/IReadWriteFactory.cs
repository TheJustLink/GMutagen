namespace GMutagen.IO;

public interface IReadWriteFactory
{
    IReadWrite<TId, TValue> CreateReadWrite<TId, TValue>() where TId : notnull;
}