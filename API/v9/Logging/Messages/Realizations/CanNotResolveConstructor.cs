using System.Linq;
using System.Reflection;
using GMutagen.v9.Resolving.Contexts;
using Logger.Extensions;
using Logger.Messages;

namespace GMutagen.v9.Logging.Messages.Realizations;

internal class CanNotResolveConstructor : MessageWithSender
{
    public CanNotResolveConstructor(Context context, ConstructorInfo constructor, object sender) : base(sender)
    {
        Placeholders
            .AddMessage($"Can not resolve: {context.Type} " +
                        $"from constructor: {constructor} " +
                        $"with parameters: " +
                        $"{
                            string.Join(", ",
                                constructor
                                    .GetParameters()
                                    .Select(p => p.ParameterType.ToString()
                                    )
                                )
                        }"
                        );
    }
}