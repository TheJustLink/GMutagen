using System;

namespace GMutagen.v6;

public interface ITypeRead<out TValue> : IRead<Type, TValue>
{
    T Read<T>() where T : class;
}