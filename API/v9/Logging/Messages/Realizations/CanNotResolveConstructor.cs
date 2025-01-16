using System.Linq;
using System.Reflection;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Logging.Messages;

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