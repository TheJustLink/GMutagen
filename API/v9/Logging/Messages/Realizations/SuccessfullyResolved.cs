using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

public class SuccessfullyResolved : MessageWithSender
{
    public SuccessfullyResolved(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully resolved: {context.Type}");
    }

}