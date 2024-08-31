namespace GMutagen.v6;

public interface IObject
{
    T Get<T>();
    bool TryGet<T>(out T contract);
}