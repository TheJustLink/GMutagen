namespace GMutagen.v6.Objects.Interface;

public interface IObject
{
    T Get<T>();
    bool TryGet<T>(out T contract);
}