namespace EventBus;

public class EventContext
{
    public Event Event { get; }
    public Topic Topic { get; }
    public bool IsHandled { get; set; }

    public EventContext(Event @event, Topic topic)
    {
        Event = @event;
        Topic = topic;
    }
}