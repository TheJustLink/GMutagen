using System.Linq;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations.Cache;

public class CachedWithKeysInContext : MessageWithSender
{
    public CachedWithKeysInContext(Context context, Context cacheContext, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully cached in context: {context} " +
                        $"with keys: " + string.Join(" ", context.Keys!.KeyTypes.Select(k => k.ToString())));
    }
}