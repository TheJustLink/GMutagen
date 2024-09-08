using GMutagen.v8.IO;
using GMutagen.v8.Objects;

namespace GMutagen.v8.Values;

public class ExternalValueGenerator<TId, TValue> : IGenerator<IValue<TValue>>
{
    private readonly IGenerator<TId> _idGenerator;
    private readonly IReadWrite<TId, TValue> _readWrite;

    public ExternalValueGenerator(IGenerator<TId> idGenerator, IReadWrite<TId, TValue> readWrite)
    {
        _idGenerator = idGenerator;
        _readWrite = readWrite;
    }

    public IValue<TValue> Generate()
    {
        var id = _idGenerator.Generate();

        return new ExternalValue<TId, TValue>(id, _readWrite);
    }
}