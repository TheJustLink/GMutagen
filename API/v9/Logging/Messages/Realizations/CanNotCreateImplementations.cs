using System;
using System.Collections.Generic;
using System.Linq;
using GMutagen.v9.Contracts.Descriptors;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

public class CanNotCreateImplementations : MessageWithSender
{
    public CanNotCreateImplementations(Context context, Dictionary<Type, ContractDescriptor> contracts,
        Dictionary<Type, object> implementations, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not create: {string.Join(", ", implementations.Keys.Select(k => k.ToString()))}");
    }
}