using GMutagen.v9.Contracts.Resolving.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Logging.Messages.Cache;

public class CachedWithoutKeysInCollection : MessageWithSender
{
    public CachedWithoutKeysInCollection(Context context, IServiceCollection cache, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully cached in collection: {cache} " +
                        $"without keys");
    }
}