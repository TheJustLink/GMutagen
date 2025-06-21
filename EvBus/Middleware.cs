namespace EventBus;

public abstract class Middleware : IMiddleware
{
    public abstract Task InvokeAsync(EventContext context, Func<Task> next);
}