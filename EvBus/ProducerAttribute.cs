namespace EventBus;

[AttributeUsage(AttributeTargets.Method)]
public class ProducerAttribute : Attribute
{
    public string Topic { get; }
    public ProducerAttribute(string topic) => Topic = topic;
}