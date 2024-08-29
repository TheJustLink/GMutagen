namespace GMutagen.v6.IO;

public interface ITypeReadWrite<out TValue> : ITypeRead<TValue>, ITypeWrite<TValue>
{
}