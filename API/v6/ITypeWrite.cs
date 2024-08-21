using System;

namespace GMutagen.v6;

public interface ITypeWrite<out TValue> : IRead<Type, TValue>
{
    void Write<T>(T value);
}