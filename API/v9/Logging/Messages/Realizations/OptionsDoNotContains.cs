using GMutagen.v9.Resolving.Contexts.Option;

namespace GMutagen.v9.Logging.Messages.Realizations;

public class OptionsDoNotContains : MessageWithSender
{
    public OptionsDoNotContains(Options? options, OptionType optionType, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Options: {options} does not contains option type: {optionType}");
    }
}