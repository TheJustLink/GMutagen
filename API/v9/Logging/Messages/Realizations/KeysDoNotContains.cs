using GMutagen.v9.Contracts.Resolving.Contexts.Key;

namespace GMutagen.v9.Logging.Messages;

public class KeysDoNotContains : MessageWithSender
{
    public KeysDoNotContains(Keys keys, KeyType idKeyType, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Keys: {keys} does not contains key type: {idKeyType}");
    }
}