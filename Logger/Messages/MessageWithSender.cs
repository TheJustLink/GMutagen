using Logger.Extensions;

namespace Logger.Messages;

public abstract class MessageWithSender : Message
{
    protected object Sender { get; }

    protected MessageWithSender(object sender)
    {
        Sender = sender;
        Placeholders
            .AddSender(sender.GetType().Name);
    }
}