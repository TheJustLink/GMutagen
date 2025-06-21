using System.Collections.Generic;
using System.Linq;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations.Cache;

public class CachedWithKeysInDictionary : MessageWithSender
{
    public CachedWithKeysInDictionary(Context context, Dictionary<object, object> cache, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully cached in dictionary: {cache} " +
                        $"with keys: " + string.Join(" ", context.Keys!.KeyTypes.Select(k => k.ToString())));
    }
}