namespace ActionFlow;

public class ConditionalFlow<T>(IFlow<T> flow, Func<T, bool> condition) : Flow<T>(flow), IConditionalFlow<T>
{
    public Func<T, bool> Condition => condition;
    
    public override void Invoke(T input)
    {
        foreach (var handler in BeforeHandlers)
        {
            handler.Invoke(input);
        }
        
        if(!condition(input))
            return;
        
        foreach (var handler in Handlers)
        {
            handler.Invoke(input);
        }
    }
}