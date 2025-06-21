namespace EventBus;

public class ActionHandler<T>(Action<T> handler) : IEventHandler<T>
    where T : Event
{
    public Task HandleAsync(T @event)
    {
        handler.Invoke(@event);
        return Task.CompletedTask;
    }
}