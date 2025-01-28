using System;
using GMutagen.v9.Resolving.Contexts;

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