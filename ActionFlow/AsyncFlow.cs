namespace ActionFlow;

public class AsyncFlow<T> : BasicFlow<IAsyncFlow<T>>, IAsyncFlow<T>
{
    public AsyncFlow(params IAsyncFlow<T>[] handlers) : base(handlers)
    {
        
    }

    public IAsyncFlow<T> Add(IAsyncFlow<T> handler)
    {
        Handlers.Add(handler);
        return this;
    }

    public IAsyncFlow<T> Remove(IAsyncFlow<T> handler)
    {
        Handlers.RemoveAll(h => h.Equals(handler));
        return this;
    }

    public virtual async Task Invoke(T input)
    {
        foreach (var handler in BeforeHandlers)
        {
            await handler.Invoke(input);
        }
        
        foreach (var handler in Handlers)
        {
            await handler.Invoke(input);
        }
    }
}