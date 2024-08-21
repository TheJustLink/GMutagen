using System;

namespace GMutagen.v6;

public class TypeRepository<TValue> : MemoryRepository<Type, TValue>, ITypeReadWrite<TValue> where TValue : class
{
    public T Read<T>() where T : class
    {
        return Read(typeof(T)) as T;
    }

    public void Write<T>(T value)
    {
        Write(typeof(T), value as TValue);
    }
}