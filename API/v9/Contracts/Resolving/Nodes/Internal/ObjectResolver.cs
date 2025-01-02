using System;
using System.Collections.Generic;
using GMutagen.v9.Generators;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.Objects;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal;

public class ObjectResolver<TObjectId, TContractId>(IResolverNode resolver, IGenerator<TContractId> generator)
    : RecursiveResolverNode(resolver) where TObjectId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IObject)))
            return false;
        
        var successGetKey = context.TryGetKey<TObjectId>(KeyType.Id, out var id);

        if (successGetKey is false)
            return false;
        
        var successGetOption = context.TryGetOption<Dictionary<Type, ContractDescriptor>>(OptionType.Contracts, 
            out var contracts);

        if (successGetOption is false)
            return false;

        var successCreation = TryCreateImplementations(contracts, context, out var implementations);

        if (successCreation is false)
            return false;
        
        var obj = new Object<TObjectId>(id, implementations);
        context.Instance = obj;
        return true;
    }
    
    private bool TryCreateImplementations(Dictionary<Type, ContractDescriptor> contracts, Context context,
        out Dictionary<Type, object> implementations)
    {
        implementations = new Dictionary<Type, object>(contracts.Count);
        
        foreach (var (type, descriptor) in contracts)
        {
            var id = generator.Generate();
            var keys = new Keys(2)
                .Add(KeyType.Id, id)
                .Add(KeyType.DeclaredType, type);
            
            var contractContext = new Context(descriptor.ImplementationType!, keys, parentContext: context);
            var success = Resolver.Resolve(contractContext);
            
            if (success)
                implementations[type] = contractContext.Instance!;
            else
                return false;
        }

        return true;
    }
}