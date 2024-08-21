namespace GMutagen.v6;

public interface ITypeReadWrite<out TValue> : ITypeRead<TValue>, ITypeWrite<TValue>
{
}