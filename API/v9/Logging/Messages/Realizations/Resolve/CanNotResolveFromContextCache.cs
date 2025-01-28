using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages.Realizations.Resolve;

public class CanNotResolveFromContextCache : MessageWithSender
{
    public CanNotResolveFromContextCache(Context currentContext, Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve {context.Type} from: {currentContext}");
    }
}