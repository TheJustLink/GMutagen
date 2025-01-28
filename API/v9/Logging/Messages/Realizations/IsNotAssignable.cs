using System;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class IsNotAssignable : MessageWithSender
{
    public IsNotAssignable(Type contextType, Type type, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"{contextType} is not assignable to {type}");
    }
}