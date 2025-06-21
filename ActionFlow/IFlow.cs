namespace ActionFlow;

public interface IFlow<T>
{
    void Invoke(T input);
    IFlow<T> Add(IFlow<T> handler);
    IFlow<T> Remove(IFlow<T> handler);
}