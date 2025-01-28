using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class SuccessfullyResolved : MessageWithSender
{
    public SuccessfullyResolved(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully resolved: {context.Type}");
    }

}