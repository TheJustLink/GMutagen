using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Realizations;

internal class CanNotResolveStorage : MessageWithSender
{
    public CanNotResolveStorage(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve storage: {context.Type}");
    }
}