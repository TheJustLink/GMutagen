using System.Linq;
using System.Reflection;
using GMutagen.v9.Contracts.Resolving.Attributes;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.Extensions;
using GMutagen.v9.IO;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class ContractFromMetaCache<TContractId, TValueId>(
    IResolverNode resolver,
    KeyType idKeyType,
    IReadWrite<TContractId, ContractMetaData<int, TValueId>> readWrite) : RecursiveResolverNode(resolver) where TContractId : notnull
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
            return false;

        var constructors = context.Type.GetConstructors();
        var isResolved = constructors.Any(constructor => ResolveConstructor(context, constructor));
        return isResolved;
    }
    
    private bool ResolveConstructor(Context context, ConstructorInfo constructor)
    {
        if (!context.TryGetKey(idKeyType, out TContractId contractId))
            return false;

        if (!readWrite.Contains(contractId))
            return false;
        
        var metaData = readWrite.Read(contractId);
        
        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];

            object? parameter;
            bool success;
            if(metaData.Slots.TryGetValue(i, out var valueId))
                success = ResolveParameter(context, parameterInfo, valueId, out parameter);
            else
                success = ResolveParameter(context, parameterInfo, out parameter);
            
            if(success is false)
                return false;

            resultParameters[i] = parameter!;
        }

        context.Instance = constructor.Invoke(resultParameters);
        return true;
    }

    private bool ResolveParameter(Context context, ParameterInfo parameterInfo, out object? parameter)
    {
        parameter = null;
        
        var keys = GetKeysFor(parameterInfo)
            .Add(KeyType.DeclaredType, parameterInfo.ParameterType);

        var options = GetOptionsFor(parameterInfo);

        var parameterContext = new Context(parameterInfo.ParameterType, keys, options, context);

        var success = Resolver.Resolve(parameterContext);
        if (success is false)
            return false;
        
        parameter = parameterContext.Instance;

        return success;
    }

    private bool ResolveParameter(Context context, ParameterInfo parameterInfo, TValueId id, out object? parameter)
    {
        parameter = null;
        
        var keys = GetKeysFor(parameterInfo)
            .Add(KeyType.Id, id)
            .Add(KeyType.DeclaredType, parameterInfo.ParameterType);

        var options = GetOptionsFor(parameterInfo);

        var parameterContext = new Context(parameterInfo.ParameterType, keys, options, context);

        var success = Resolver.Resolve(parameterContext);
        if (success is false)
            return false;
        
        parameter = parameterContext.Instance;

        return success;
    }

    private Keys GetKeysFor(ParameterInfo parameterInfo)
    {
        var key = new Keys();

        var attributes = parameterInfo.GetCustomAttributes();
        if (attributes.Contains<IdAttribute>())
        {
            var idAttribute = attributes.Get<IdAttribute>();
            key.Add(KeyType.Id, idAttribute.Id);
        }

        return key;
    }

    private Options GetOptionsFor(ParameterInfo parameterInfo)
    {
        Options options = new Options();

        var attributes = parameterInfo.GetCustomAttributes();
        if (attributes.Contains<ValueLocationAttribute>())
        {
            var valueLocationAttribute = attributes.Get<ValueLocationAttribute>();
            options.Add(OptionType.Location, valueLocationAttribute.LocationType);
        }

        return options;
    }
}