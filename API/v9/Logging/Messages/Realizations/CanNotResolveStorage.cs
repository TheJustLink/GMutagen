using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Logging.Messages;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal.Resolvers;

internal class CanNotResolveStorage : MessageWithSender
{
    public CanNotResolveStorage(Context context, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve storage: {context.Type}");
    }
}