using System.Linq;
using GMutagen.v9.Contracts.Resolving.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Logging.Messages.Cache;

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