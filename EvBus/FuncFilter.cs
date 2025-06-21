namespace EventBus;

public class FuncFilter<T> : IEventFilter<T> where T : Event
{
    private readonly Func<T, bool> _filter;

    public FuncFilter(Func<T, bool> filter)
    {
        _filter = filter;
    }

    public bool Filter(T @event)
    {
        return _filter.Invoke(@event);
    }
}