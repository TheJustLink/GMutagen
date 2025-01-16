using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

public class FailedToResolve : MessageWithSender
{
    public FailedToResolve(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Failed to resolve: {context.Type}");
    }
}