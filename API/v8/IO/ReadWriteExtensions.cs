using GMutagen.v8.Objects;
using GMutagen.v8.Values;

namespace GMutagen.v8.IO;

public static class ReadWriteExtensions
{
    public static ExternalValue<TId, TValue> Instantiate<TId, TValue>(this IReadWrite<TId, TValue> readWrite,
        IGenerator<TId> idGenerator)
    {
        return new ExternalValue<TId, TValue>(idGenerator.Generate(), readWrite);
    }

    public static ExternalValue<TId, TValue> GetInstance<TId, TValue>(this IReadWrite<TId, TValue> readWrite, TId id)
    {
        return new ExternalValue<TId, TValue>(id, readWrite);
    }
}