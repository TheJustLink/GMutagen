using GMutagen.v9.IO.Interfaces;

namespace GMutagen.v9.IO.Factories.Interfaces;

public interface IReadWriteFactory
{
    IReadWrite<TId, TValue> CreateReadWrite<TId, TValue>() where TId : notnull;
}