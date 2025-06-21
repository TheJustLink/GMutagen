namespace EventBus;

public class ReplicationBuilder(EventBus eventBus, Topic sourceTopic)
{
    private readonly List<Topic> _targetTopics = new();

    public ReplicationBuilder To(Topic targetTopic)
    {
        _targetTopics.Add(targetTopic);
        return this;
    }

    public IDisposable Build()
    {
        return eventBus.Replicate(sourceTopic, _targetTopics.ToArray());
    }
}