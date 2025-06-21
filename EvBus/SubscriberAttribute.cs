namespace EventBus;

[AttributeUsage(AttributeTargets.Method)]
public class SubscriberAttribute : Attribute
{
    public string Topic { get; }
    public SubscriberAttribute(string topic) => Topic = topic;
}