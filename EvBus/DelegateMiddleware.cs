namespace EventBus;

public class DelegateMiddleware : IMiddleware
{
    private readonly Func<EventContext, Func<Task>, Task> _func;

    public DelegateMiddleware(Func<EventContext, Func<Task>, Task> func)
    {
        _func = func;
    }

    public async Task InvokeAsync(EventContext context, Func<Task> next)
    {
        await _func(context, next);
    }
}