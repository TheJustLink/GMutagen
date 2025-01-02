using System.Linq;
using System.Reflection;
using GMutagen.v9.Contracts.Resolving.Attributes;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Contexts.Option;
using GMutagen.v9.Contracts.Resolving.Nodes.From;
using GMutagen.v9.Extensions;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal;

public class ContractResolver(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!typeof(IContract).IsAssignableTo(context.Type))
            return false;
        
        var constructors = context.Type.GetConstructors();
        var isResolved = constructors.Any(constructor => ResolveConstructor(context, constructor));
        return isResolved;
    }

    private bool ResolveConstructor(Context context, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];

            var success = ResolveParameter(context, parameterInfo, i, out var parameter);
            
            if(success is false)
                return false;

            resultParameters[i] = parameter!;
        }

        context.Instance = constructor.Invoke(resultParameters);
        return true;
    }

    private bool ResolveParameter(Context context, ParameterInfo parameterInfo, int index, out object? parameter)
    {
        var keys = GetKeysFor(parameterInfo)
            .Add(KeyType.Index, index);

        var options = GetOptionsFor(parameterInfo);

        var parameterContext = new Context(parameterInfo.ParameterType, keys, options, context);

        var success = Resolver.Resolve(parameterContext);
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
            options.Add(OptionType.Location, valueLocationAttribute.GetType());
        }

        return options;
    }
}