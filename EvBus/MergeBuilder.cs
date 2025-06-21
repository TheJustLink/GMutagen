namespace EventBus;

public class MergeBuilder(EventBus eventBus, Topic targetTopic)
{
    private readonly List<Topic> _sourceTopics = new();

    public MergeBuilder From(Topic sourceTopic)
    {
        _sourceTopics.Add(sourceTopic);
        return this;
    }

    public IDisposable Build()
    {
        return eventBus.MergeTopics(targetTopic, _sourceTopics.ToArray());
    }
}