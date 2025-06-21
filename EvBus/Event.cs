using Logger.Logger;

namespace EventBus;

public class Event
{
    public string Topic { get; set; }
    public EventBus EventBus { get; set; }

    public Event()
    {
        Topic = string.Empty;
        EventBus = Static.EventBus;
    }

    public async Task Fire()
    {
        await EventBus.Publish(this, Topic);
    }

    public void Subscribe(Action action)
    {
        Subscribe<Event>((ev) => action());
    }
    
    public void Subscribe<TEvent>(Action<TEvent> action) where TEvent : Event
    {
        EventBus.Subscribe(Topic, action);
    }
}

public class Static
{
    public static EventBus EventBus = new(new List<IMiddleware>(), new ConsoleLogger<EventBus>());
}