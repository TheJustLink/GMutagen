using System.Reflection;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations;

internal class SuccessfulResolvedParameter : MessageWithSender
{
    public SuccessfulResolvedParameter(Context context, int i, ParameterInfo parameterInfo, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Successfully resolved parameter: {context.Type} " +
                        $"with name {parameterInfo.Name} " +
                        $"at index: {i} " +
                        $"for {context.Type}");
    }

}