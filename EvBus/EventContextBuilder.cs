namespace EventBus;

public class EventContextBuilder
{
    private Event _event = default!;
    private Topic _topic = default!;

    public EventContextBuilder WithEvent(Event @event)
    {
        _event = @event;
        return this;
    }

    public EventContextBuilder WithTopic(Topic topic)
    {
        _topic = topic;
        return this;
    }

    public EventContext Build()
    {
        return new EventContext(_event, _topic);
    }
}