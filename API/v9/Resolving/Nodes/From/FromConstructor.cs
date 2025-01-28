using System.Reflection;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages.Realizations;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.From;

public class FromConstructor(IResolverNode resolver, ILogger<Global> logger) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
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
                return true;
            
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

            if (!ResolveParameter(context, parameterInfo, out var parameter))
            {
                logger.LogWarning(new CanNotResolveParameter(context, i, parameterInfo, this));
                return false;
            }

            resultParameters[i] = parameter;
            logger.LogInfo(new SuccessfulResolvedParameter(context, i, parameterInfo, this));
        }

        context.Instance = constructor.Invoke(resultParameters);
        logger.LogInfo(new SuccessfullyResolved(context, this));
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