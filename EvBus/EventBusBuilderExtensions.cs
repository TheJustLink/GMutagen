namespace EventBus;

public static class EventBusBuilderExtensions
{
    public static ReplicationBuilder AddReplicateGroup(this EventBus eventBus, Topic sourceTopic)
    {
        return new ReplicationBuilder(eventBus, sourceTopic);
    }

    public static MergeBuilder AddMergeGroup(this EventBus eventBus, Topic targetTopic)
    {
        return new MergeBuilder(eventBus, targetTopic);
    }
}