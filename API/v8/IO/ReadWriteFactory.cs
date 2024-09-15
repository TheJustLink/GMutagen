namespace GMutagen.v8.IO;

public class ReadWriteFactory : IReadWriteFactory
{
    private readonly IReadFactory _readFactory;
    private readonly IWriteFactory _writeFactory;
    public ReadWriteFactory(IReadFactory readFactory, IWriteFactory writeFactory)
    {
        _readFactory = readFactory;
        _writeFactory = writeFactory;
    }

    public IReadWrite<TId, TValue> CreateReadWrite<TId, TValue>() where TId : notnull
    {
        var reader = _readFactory.CreateRead<TId, TValue>();
        var writer = _writeFactory.CreateWrite<TId, TValue>();

        return new ReadWrite<TId, TValue>(reader, writer);
    }
}
public class ReadWriteFactory<TReadFactory, TWriteFactory> : IReadWriteFactory
    where TReadFactory : IReadFactory, new()
    where TWriteFactory : IWriteFactory, new()
{
    public IReadWrite<TId, TValue> CreateReadWrite<TId, TValue>() where TId : notnull
    {
        var reader = new TReadFactory().CreateRead<TId, TValue>();
        var writer = new TWriteFactory().CreateWrite<TId, TValue>();

        return new ReadWrite<TId, TValue>(reader, writer);
    }
}