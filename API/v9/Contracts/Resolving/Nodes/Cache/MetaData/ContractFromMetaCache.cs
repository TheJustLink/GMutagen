using System.Reflection;
using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.Contracts.Resolving.Attributes;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.Decorator;
using GMutagen.v9.Contracts.Resolving.Nodes.Interfaces;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Models;
using GMutagen.v9.Extensions;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Cache.MetaData;

public class ContractFromMetaCache<TContractId, TValueId>(
    IResolverNode resolver,
    KeyType idKeyType,
    IReadWrite<TContractId, ContractMetaData<int, TValueId>> readWrite,
    ILogger<Global> logger) : RecursiveResolverNode(resolver) where TContractId : notnull
{
    public override bool Resolve(Contexts.Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
        {
            logger.LogWarning(new IsNotAssignable(context.Type, typeof(IContract), this));
            return false;
        }

        var constructors = context.Type.GetConstructors();
        var isResolved = ResolveConstructors(context, constructors);
        return isResolved;
    }

    private bool ResolveConstructors(Contexts.Context context, ConstructorInfo[] constructors)
    {
        foreach (var constructor in constructors)
        {
            var success = ResolveConstructor(context, constructor);

            if (success)
            {
                return true;
            }

            logger.LogWarning(new CanNotResolveConstructor(context, constructor, this));
        }

        return false;
    }

    private bool ResolveConstructor(Contexts.Context context, ConstructorInfo constructor)
    {
        if (!context.TryGetKey(idKeyType, out TContractId contractId))
        {
            logger.LogWarning(new KeysDoNotContains(context.Keys, idKeyType, this));
            return false;
        }

        if (!readWrite.Contains(contractId))
        {
            logger.LogWarning(
                new ReadWriteDoNotContains<IReadWrite<TContractId, ContractMetaData<int, TValueId>>>(readWrite,
                    contractId, this));
            return false;
        }

        var metaData = readWrite.Read(contractId);

        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];

            object? parameter;
            bool success;
            if (metaData.Slots.TryGetValue(i, out var valueId))
                success = ResolveParameter(context, parameterInfo, valueId, out parameter);
            else
                success = ResolveParameter(context, parameterInfo, out parameter);

            if (success is false)
            {
                logger.LogWarning(new CanNotResolveParameter(context, i, parameterInfo, this));
                return false;
            }

            logger.LogInfo(new SuccessfulResolvedParameter(context, i, parameterInfo, this));
            resultParameters[i] = parameter!;
        }

        context.Instance = constructor.Invoke(resultParameters);
        logger.LogInfo(new SuccessfullyResolved(context, this));
        return true;
    }

    private bool ResolveParameter(Contexts.Context context, ParameterInfo parameterInfo, out object? parameter)
    {
        parameter = null;

        var keys = GetKeysFor(parameterInfo)
            .Add(KeyType.DeclaredType, parameterInfo.ParameterType);

        var options = GetOptionsFor(parameterInfo);

        var parameterContext = new Contexts.Context(parameterInfo.ParameterType, keys, options, context);

        var success = Resolver.Resolve(parameterContext);
        if (success is false)
            return false;

        parameter = parameterContext.Instance;

        return success;
    }

    private bool ResolveParameter(Contexts.Context context, ParameterInfo parameterInfo, TValueId id, out object? parameter)
    {
        parameter = null;

        var keys = GetKeysFor(parameterInfo)
            .Add(KeyType.Id, id)
            .Add(KeyType.DeclaredType, parameterInfo.ParameterType);

        var options = GetOptionsFor(parameterInfo);

        var parameterContext = new Contexts.Context(parameterInfo.ParameterType, keys, options, context);

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