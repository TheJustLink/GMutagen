using System;
using System.Collections.Generic;
using GMutagen.Generators;
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
        if (!typeof(IObject).IsAssignableTo(context.Type))
            return false;
        
        var successGetKey = context.TryGetKey<TObjectId>(KeyType.Id, out var id);

        if (successGetKey is false)
            return false;
        
        var successGetOption = context.TryGetOption<Dictionary<Type, ContractDescriptor>>(OptionType.Contracts, 
            out var contracts);

        if (successGetOption is false)
            return false;

        var implementations = CreateImplementations(contracts);
        var obj = new Object<TObjectId>(id, implementations);
        context.Instance = obj;
        return true;
    }
    
    private Dictionary<Type, object> CreateImplementations(Dictionary<Type, ContractDescriptor> contracts)
    {
        var implementations = new Dictionary<Type, object>(contracts.Count);
        
        foreach (var (type, _) in contracts)
        {
            var id = generator.Generate();
            var keys = new Keys(1).Add(KeyType.Id, id);
            
            var context = new Context(type, keys);
            implementations[type] = Resolver.Resolve(context);
        }

        return implementations;
    }
}