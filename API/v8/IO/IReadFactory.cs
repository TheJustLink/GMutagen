namespace GMutagen.v8.IO;

public interface IReadFactory
{
    IRead<TId, TValue> CreateRead<TId, TValue>() where TId : notnull;
}