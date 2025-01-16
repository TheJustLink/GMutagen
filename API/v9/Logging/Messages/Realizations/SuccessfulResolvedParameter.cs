using System.Reflection;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

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