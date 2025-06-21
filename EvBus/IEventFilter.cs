namespace EventBus;

public interface IEventFilter<in TEvent> where TEvent : Event
{
    bool Filter(TEvent @event);
}