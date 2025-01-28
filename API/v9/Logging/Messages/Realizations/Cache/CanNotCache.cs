using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Realizations.Cache;

public class CanNotCache : MessageWithSender
{
    public CanNotCache(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Filed to cache");
    }
}