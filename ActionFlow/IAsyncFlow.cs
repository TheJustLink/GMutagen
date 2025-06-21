namespace ActionFlow;

public interface IAsyncFlow<T>
{
    Task Invoke(T input);
    IAsyncFlow<T> Add(IAsyncFlow<T> handler);
    IAsyncFlow<T> Remove(IAsyncFlow<T> handler);
}