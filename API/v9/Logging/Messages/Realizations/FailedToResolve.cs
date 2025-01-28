using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class FailedToResolve : MessageWithSender
{
    public FailedToResolve(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Failed to resolve: {context.Type}");
    }
}