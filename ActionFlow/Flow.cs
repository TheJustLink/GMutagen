namespace ActionFlow;

public class Flow<T>(params IFlow<T>[] flows) : BasicFlow<IFlow<T>>(flows), IFlow<T>
{
    public IFlow<T> Add(IFlow<T> handler)
    {
        Handlers.Add(handler);
        return this;
    }

    public IFlow<T> Remove(IFlow<T> handler)
    {
        Handlers.RemoveAll(h => h.Equals(handler));
        return this;
    }

    public virtual void Invoke(T input)
    {
        foreach (var handler in BeforeHandlers)
        {
            handler.Invoke(input);
        }
        
        foreach (var handler in Handlers)
        {
            handler.Invoke(input);
        }
    }
}