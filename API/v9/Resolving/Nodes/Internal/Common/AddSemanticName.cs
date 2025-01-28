using System.Reflection;
using GMutagen.v9.Extensions;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.Internal.Resolvers;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public class AddSemanticName(IConfiguration<string, string> semanticNames) : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (TryAddSemanticNameFromConfiguration(context))
            return false;
        
        if (TryAddSemanticNameFromInfo(context))
            return false;
        
        return false;
    }

    private bool TryAddSemanticNameFromConfiguration(Context context)
    {
        var id = string.Join(".", context.ParentContext!.Type.Name, context.Type.Name);
        if (!semanticNames.TryGetValue(id, out var name))
            return false;
        
        context.Options!.Add(OptionType.SemanticName, name);
        return true;
    }

    private bool TryAddSemanticNameFromInfo(Context context)
    {
        if(!context.TryGetOption(OptionType.Info, out ParameterInfo parameterInfo))
            return false;

        var attributes = parameterInfo.GetCustomAttributes();
        
        if (attributes.Contains<SemanticNameAttribute>())
        {
            var semanticNameAttribute = attributes.Get<SemanticNameAttribute>();
            context.Options!.Add(OptionType.SemanticName, semanticNameAttribute!.Name);
        }

        return true;
    }
}