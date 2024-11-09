namespace GMutagen.IO;

public interface IReadFactory
{
    IRead<TId, TValue> CreateRead<TId, TValue>() where TId : notnull;
}