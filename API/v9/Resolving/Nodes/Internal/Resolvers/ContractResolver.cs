using System.Reflection;
using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.Extensions;
using GMutagen.v9.Generators.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Attributes;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Key;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using Logger.Logger.Common;
using Logger.Logger.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Internal.Resolvers;

public class ContractResolver<TValueId>(
    IResolverNode resolver,
    IGenerator<TValueId> valueIdGenerator,
    ILogger<Global> logger)
    : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
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

    private bool ResolveConstructors(Context context, ConstructorInfo[] constructors)
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

    private bool ResolveConstructor(Context context, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];

            var success = ResolveParameter(context, parameterInfo, out var parameter, i);

            if (success is false)
            {
                logger.LogWarning(new CanNotResolveParameter(context, i, parameterInfo, this));
                return false;
            }

            resultParameters[i] = parameter!;
            logger.LogInfo(new SuccessfulResolvedParameter(context, i, parameterInfo, this));
        }

        context.Instance = constructor.Invoke(resultParameters);
        logger.LogInfo(new SuccessfullyResolved(context, this));
        return true;
    }

    private bool ResolveParameter(Context context, ParameterInfo parameterInfo, out object? parameter, int i)
    {
        parameter = null;

        var id = valueIdGenerator.Generate();

        var keys = GetKeysFor(parameterInfo)
            .Add(KeyType.Id, id)
            .Add(KeyType.Index, i)
            .Add(KeyType.DeclaredType, parameterInfo.ParameterType);

        var options = GetOptionsFor(parameterInfo)
            .Add(OptionType.Info, parameterInfo);

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