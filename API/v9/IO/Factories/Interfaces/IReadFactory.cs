using GMutagen.v9.IO.Interfaces;

namespace GMutagen.v9.IO.Factories.Interfaces;

public interface IReadFactory
{
    IRead<TId, TValue> CreateRead<TId, TValue>() where TId : notnull;
}