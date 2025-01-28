using System.Collections.Generic;
using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Realizations;

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