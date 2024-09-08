using System;

namespace GMutagen.v8.IO;

public interface ITypeWrite<out TValue> : IRead<Type, TValue>
{
    void Write<T>(T value);
}