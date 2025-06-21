using System;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class CanNotResolveMetaData : MessageWithSender
{
    public CanNotResolveMetaData(Context contractMetaDataContext, Type type, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve meta data of type: {type} " +
                        $"for: {contractMetaDataContext.Type}");
    }
}