using System.Linq;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Cache;

public class CachedWithKeysInContext : MessageWithSender
{
    public CachedWithKeysInContext(Context context, Context cacheContext, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully cached in context: {context} " +
                        $"with keys: " + string.Join(" ", context.Keys!.KeyTypes.Select(k => k.ToString())));
    }
}