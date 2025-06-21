using System;
using System.Collections.Generic;
using System.Linq;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class CanNotCreateReversedImplementations : MessageWithSender
{
    public CanNotCreateReversedImplementations(Context context, Dictionary<Type, List<Type>> contracts,
        Dictionary<Type, object> implementations, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not create: {string.Join(", ", implementations.Keys.Select(k => k.ToString()))}");
    }
}