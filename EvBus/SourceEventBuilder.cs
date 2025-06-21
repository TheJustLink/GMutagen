namespace EventBus;

public class SourceEventBuilder
{
    private readonly SourceEvent _event = new();

    public SourceEventBuilder WithTopic(string topic)
    {
        _event.Topic = topic;
        return this;
    }

    public SourceEventBuilder WithSource(Identity.Compose.Identity source)
    {
        _event.Source = source;
        return this;
    }

    public SourceEvent Build() => _event;
}