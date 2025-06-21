namespace EventBus;

public class TopicBuilder
{
    private readonly List<string> _parts = new();

    public TopicBuilder Add(string part)
    {
        _parts.Add(part);
        return this;
    }

    public TopicBuilder AddWildcard() => Add("*");
    public TopicBuilder AddMultiLevelWildcard() => Add("#");

    public Topic Build() => new(string.Join("/", _parts));
}