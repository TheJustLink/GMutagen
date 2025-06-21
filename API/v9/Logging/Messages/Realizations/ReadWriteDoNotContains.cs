using GMutagen.v9.IO.Interfaces;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class ReadWriteDoNotContains<T> : MessageWithSender where T : IReadWrite
{
    public ReadWriteDoNotContains(T readWrite, object key, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"ReadWrite: {readWrite} does not contains id: {key}");
    }
}