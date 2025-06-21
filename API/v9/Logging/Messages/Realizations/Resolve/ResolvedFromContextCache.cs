using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations.Resolve;

public class ResolvedFromContextCache : MessageWithSender
{
    public ResolvedFromContextCache(Context currentContext, Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Resolved {context.Type} from: {currentContext}");
    }
}