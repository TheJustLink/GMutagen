using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Cache;

public class CanNotCache : MessageWithSender
{
    public CanNotCache(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Filed to cache");
    }
}