namespace EventBus;

public interface IMiddleware
{
    Task InvokeAsync(EventContext context, Func<Task> next);
}