using GMutagen.v9.Resolving.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Logging.Messages.Realizations.Cache;

public class CachedWithoutKeysInCollection : MessageWithSender
{
    public CachedWithoutKeysInCollection(Context context, IServiceCollection cache, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully cached in collection: {cache} " +
                        $"without keys");
    }
}