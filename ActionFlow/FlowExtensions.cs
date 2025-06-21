namespace ActionFlow;

public static class FlowExtensions
{
    public static IFlow<T> AsFlow<T>(this IFlow<T> flow) => flow;
    public static IAsyncFlow<T> AsAsyncFlow<T>(this IAsyncFlow<T> flow) => flow;

    public static IFlow<T> AsFlow<T>(this Action<T> action)
    {
        var flow = new Flow<T>();
        flow.Add(new ActionFlow<T>(action));
        return flow;
    }

    public static IFlow<T> When<T>(
        this IFlow<T> flow,
        Func<T, bool> condition)
    {
        return new ConditionalFlow<T>(flow, condition);
    }

    public static IAsyncFlow<T> AsAsyncFlow<T>(this Action<T> func)
    {
        var flow = new AsyncFlow<T>();
        flow.Add(new AsyncActionFlow<T>((data) =>
        {
            func(data);
            return Task.CompletedTask;
        }));
        return flow;
    }
    
    public static IAsyncFlow<T> AsAsyncFlow<T>(this Func<T, Task> func)
    {
        var flow = new AsyncFlow<T>();
        flow.Add(new AsyncActionFlow<T>(func));
        return flow;
    }

    public static IAsyncFlow<T> When<T>(
        this IAsyncFlow<T> flow,
        Func<T, bool> condition)
    {
        return new ConditionalAsyncFlow<T>(condition, flow);
    }
}

public class AsyncActionFlow<T>(Func<T, Task> func) : IAsyncFlow<T>
{
    public Task Invoke(T input)
    {
        return func(input);
    }

    public IAsyncFlow<T> Add(IAsyncFlow<T> handler)
    {
        func += handler.Invoke;
        return this;
    }

    public IAsyncFlow<T> Remove(IAsyncFlow<T> handler)
    {
        func -= handler.Invoke;
        return this;
    }
}

public class ActionFlow<T>(Action<T> action) : IFlow<T>
{
    public void Invoke(T input)
    {
        action(input);
    }

    public IFlow<T> Add(IFlow<T> handler)
    {
        action += handler.Invoke;
        return this;
    }

    public IFlow<T> Remove(IFlow<T> handler)
    {
        action -= handler.Invoke;
        return this;
    }
}