using System;
using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Realizations;

internal class CanNotResolveFromServicesCache : MessageWithSender
{
    public CanNotResolveFromServicesCache(IServiceProvider provider, Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve: {context.Type} " +
                        $"from services: {provider}");
    }
}