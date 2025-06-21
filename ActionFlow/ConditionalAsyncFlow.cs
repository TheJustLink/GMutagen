namespace ActionFlow;

public class ConditionalAsyncFlow<T>(Func<T, bool> condition, params IAsyncFlow<T>[] flows) : AsyncFlow<T>(flows), IAsyncConditionalFlow<T>
{
    public Func<T, bool> Condition => condition;

    public override async Task Invoke(T input)
    {
        foreach (var handler in BeforeHandlers)
        {
            await handler.Invoke(input);
        }
        
        if(!condition(input))
            return;
        
        foreach (var handler in Handlers)
        {
            await handler.Invoke(input);
        }
    }
}