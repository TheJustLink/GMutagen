using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.From;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal;

public class MapContractInterface(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
            return false;

        var parentContext = context.ParentContext;
        var rootContext = context;
        
        while (parentContext != null)
        {
            rootContext = parentContext;
            parentContext = parentContext.ParentContext;
        }
        
        var success = rootContext.TryGetOption<Dictionary<Type, ContractDescriptor>>
                (OptionType.Contracts, out var descriptors);
        
        if (success is false)
            return false;

        success = descriptors.TryGetValue(context.Type, out var descriptor);
        if (success is false)
            return false;
        
        var newContext = Context.From(context, descriptor!.ImplementationType);

        success = Resolver.Resolve(newContext);
        context.Instance = newContext.Instance;
        return success;
    }
}