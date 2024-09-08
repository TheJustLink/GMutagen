namespace GMutagen.v8.IO;

public interface ITypeReadWrite<out TValue> : ITypeRead<TValue>, ITypeWrite<TValue>
{
}