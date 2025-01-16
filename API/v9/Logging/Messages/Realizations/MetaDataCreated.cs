using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

public class MetaDataCreated<T> : MessageWithSender
{
    public MetaDataCreated(Context context, T metaData, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Metadata: {metaData} " +
                        $"was created for: {context.Type}");
    }
}