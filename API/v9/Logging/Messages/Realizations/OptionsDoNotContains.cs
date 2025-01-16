using GMutagen.v9.Contracts.Resolving.Contexts.Option;

namespace GMutagen.v9.Logging.Messages;

public class OptionsDoNotContains : MessageWithSender
{
    public OptionsDoNotContains(Options? options, OptionType optionType, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Options: {options} does not contains option type: {optionType}");
    }
}