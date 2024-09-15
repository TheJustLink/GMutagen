using GMutagen.v8.IO;

namespace GMutagen.v8.Extensions;

public static class ReadExtensions
{
    public static bool TryGet<TId, TValue>(this IRead<TId, TValue> reader, TId id, out TValue value) where TId : notnull
    {
        value = default!;

        if (reader.Contains(id) is false)
            return false;

        value = reader.Read(id);
        return true;
    }
}