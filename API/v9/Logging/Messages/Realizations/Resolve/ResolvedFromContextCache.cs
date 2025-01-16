using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Resolve;

public class ResolvedFromContextCache : MessageWithSender
{
    public ResolvedFromContextCache(Context currentContext, Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Resolved {context.Type} from: {currentContext}");
    }
}