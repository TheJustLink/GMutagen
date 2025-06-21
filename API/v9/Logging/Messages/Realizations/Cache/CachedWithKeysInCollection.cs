using System.Linq;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Logging.Messages.Realizations.Cache;

public class CachedWithKeysInCollection : MessageWithSender
{
    public CachedWithKeysInCollection(Context context, IServiceCollection collection, object sender) :
        base(sender)
    {
        Placeholders
            .AddMessage($"Successfully cached in collection: {collection} " +
                        $"with keys: " + string.Join(" ", context.Keys!.KeyTypes.Select(k => k.ToString())));
    }
}