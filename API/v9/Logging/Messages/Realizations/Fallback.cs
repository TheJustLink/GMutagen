namespace GMutagen.v9.Logging.Messages.Realizations;

public class Fallback<T> : MessageWithSender
{
    public Fallback(object source, T defaultValue, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Could not find value " +
                        $"in {source}, " +
                        $"fallback to: {defaultValue}");
    }
}