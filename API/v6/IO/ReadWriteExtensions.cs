using GMutagen.v6.Values;

namespace GMutagen.v6.IO;

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