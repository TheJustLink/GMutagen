using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Test;

public class ContractResolver : IContractResolverChain
{
    private ServiceProvider _serviceProvider;
    private object? _resolveKey;

    public ContractResolver(ServiceProvider serviceProvider) : this(serviceProvider, null)
    {
    }

    public ContractResolver(ServiceProvider serviceProvider, object? resolveKey)
    {
        _serviceProvider = serviceProvider;
        _resolveKey = resolveKey;
    }

    public bool Resolve(ContractResolverContext context)
    {
        object? result = null;
        if (_resolveKey != null)
            result = _serviceProvider.GetRequiredKeyedService(context.Contract.ImplementationType!, _resolveKey);

        result = _serviceProvider.GetRequiredService(context.Contract.ImplementationType!);
        return true;
    }
}