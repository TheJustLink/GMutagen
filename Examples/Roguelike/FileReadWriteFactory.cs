using GMutagen.v8.IO;

namespace Roguelike;

public class FileReadWriteFactory : IReadWriteFactory
{
    private readonly IReadWriteFactory _sourceStorageFactory;
    private readonly string _defaultFilePath;
    public FileReadWriteFactory(IReadWriteFactory sourceStorageFactory, string defaultFilePath)
    {
        _sourceStorageFactory = sourceStorageFactory;
        _defaultFilePath = defaultFilePath;
    }

    public IReadWrite<TId, TValue> CreateReadWrite<TId, TValue>() where TId : notnull
    {
        var source = _sourceStorageFactory.CreateReadWrite<TId, TValue>();

        return new FileRepository<TId, TValue>(source, _defaultFilePath);
    }
    public IReadWrite<TId, TValue> Create<TId, TValue>(string filePath) where TId : notnull
    {
        var source = _sourceStorageFactory.CreateReadWrite<TId, TValue>();

        return new FileRepository<TId, TValue>(source, filePath);
    }
}