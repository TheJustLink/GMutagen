namespace GMutagen.v9.Logging.Messages.Realizations;

public class Created<T> : MessageWithSender
{
    public Created(T obj, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Created: {obj} " +
                        $"of type: {typeof(T)}");
    }
}