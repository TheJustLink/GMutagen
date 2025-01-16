using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

internal class CanNotResolveFromDictionaryCache : MessageWithSender
{
    public CanNotResolveFromDictionaryCache(Dictionary<object, object> cache, Context context, object sender) :
        base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve: {context.Type} " +
                        $"from dictionary: {cache}");
    }
}