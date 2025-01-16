using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Resolve;

public class CanNotResolveFromContextCache : MessageWithSender
{
    public CanNotResolveFromContextCache(Context currentContext, Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve {context.Type} from: {currentContext}");
    }
}