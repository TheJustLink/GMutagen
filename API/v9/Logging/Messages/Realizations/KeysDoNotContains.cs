using GMutagen.v9.Resolving.Contexts.Key;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class KeysDoNotContains : MessageWithSender
{
    public KeysDoNotContains(Keys keys, KeyType idKeyType, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Keys: {keys} does not contains key type: {idKeyType}");
    }
}