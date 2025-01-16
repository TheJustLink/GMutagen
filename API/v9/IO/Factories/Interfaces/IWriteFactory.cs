using GMutagen.v9.IO.Interfaces;

namespace GMutagen.v9.IO.Factories.Interfaces;

public interface IWriteFactory
{
    IWrite<TId, TValue> CreateWrite<TId, TValue>() where TId : notnull;
}