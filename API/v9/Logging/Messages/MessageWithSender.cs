using GMutagen.v9.Logging.Messages.Realizations;

namespace GMutagen.v9.Logging.Messages;

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