using System;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

public class CanNotResolveMetaData : MessageWithSender
{
    public CanNotResolveMetaData(Context contractMetaDataContext, Type type, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve meta data of type: {type} " +
                        $"for: {contractMetaDataContext.Type}");
    }
}