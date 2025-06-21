using System;
using System.Collections.Generic;
using System.Linq;
using GMutagen.v9.Contracts.Descriptors;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class CanNotMapContract : MessageWithSender
{
    public CanNotMapContract(Context context, Dictionary<Type, ContractDescriptor> descriptors, object sender) :
        base(sender)
    {
        Placeholders
            .AddMessage($"Can not map type: {context.Type} " +
                        $"to any descriptor from: \n" +
                        $"{string.Join("\n", 
                            descriptors.Values.Select(d => 
                            $"{d.Type} : {d.ImplementationType}"
                            ))}");
    }
}