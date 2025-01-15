using System;
using System.Collections.Generic;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.IO;
using GMutagen.v9.Objects;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class ObjectFromMetaCache<TId>(
    IResolverNode resolver,
    KeyType idKeyType,
    IReadWrite<TId, ObjectMetaData<TId>> readWrite) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IObject)))
        {
            Logger.LogFailure(new Assigment(typeof(IObject)));
            return false;
        }

        if (!context.TryGetKey(idKeyType, out TId id))
        {
            Logger.LogFailure(new GetKey(idKeyType));
            return false;
        }

        if (!readWrite.Contains(id))
            return false;
        
        var metaData = readWrite.Read(id);

        var successGetOption = context.TryGetOption<Dictionary<Type, List<Type>>>(OptionType.ContractsReversed,
            out var contracts);

        if (successGetOption is false)
            return false;

        var successCreation = TryCreateImplementations(metaData, context, contracts, out var implementations);

        if (successCreation is false)
            return false;

        var obj = new Object<TId>(id, implementations);
        context.Instance = obj;
        return true;
    }

    private bool TryCreateImplementations(ObjectMetaData<TId> metaData, Context context,
        Dictionary<Type, List<Type>> contracts,
        out Dictionary<Type, object> implementations)
    {
        implementations = new Dictionary<Type, object>(metaData.Contracts.Count);

        foreach (var (implementationType, id) in metaData.Contracts)
        {
            var keys = new Keys(2)
                .Add(KeyType.Id, id)
                .Add(KeyType.DeclaredType, implementationType);

            var contractContext = new Context(implementationType!, keys, parentContext: context);
            var success = Resolver.Resolve(contractContext);


            var types = contracts[implementationType];
            if (success)
            {
                foreach (var type in types)
                {
                    implementations[type] = contractContext.Instance!;
                }
            }
            else
                return false;
        }

        return true;
    }
}

public class GetKey : IMessage
{
    public GetKey(KeyType idKeyType)
    {
        
    }

    public string Message()
    {
        return String.Empty;
    }
}

public class Logger
{
    public static void LogFailure(IMessage assigment)
    {
        
    }
}

public class Assigment(Type type) : IMessage
{
    public string Message()
    {
        return $"assigment to {type.Name}";
    }
}

public interface IMessage
{
    string Message();
}