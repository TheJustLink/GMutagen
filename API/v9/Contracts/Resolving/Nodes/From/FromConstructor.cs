using System.Linq;
using System.Reflection;
using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.From;

public class FromConstructor(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
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

            if (!ResolveParameter(context, parameterInfo,  out var parameter))
                return false;

            resultParameters[i] = parameter;
        }

        context.Instance = constructor.Invoke(resultParameters);
        return true;
    }

    private bool ResolveParameter(Context context, ParameterInfo parameterInfo, out object? parameter)
    {
        var parameterContext = new Context(parameterInfo.ParameterType, parentContext: context);
        
        var success = Resolver.Resolve(parameterContext);
        parameter = parameterContext.Instance;
        
        return success;
    }
}