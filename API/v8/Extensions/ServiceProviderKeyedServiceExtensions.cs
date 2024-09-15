using System;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Extensions;

/// <summary>
/// Extension methods for getting services from an <see cref="T:System.IServiceProvider" />.
/// </summary>
public static class ServiceProviderKeyedServiceExtensions
{
    /// <summary>
    /// Get service of type <typeparamref name="T" /> from the <see cref="T:System.IServiceProvider" />.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="provider">The <see cref="T:System.IServiceProvider" /> to retrieve the service object from.</param>
    /// <param name="serviceKey">An object that specifies the key of service object to get.</param>
    /// <returns>A service object of type <typeparamref name="T" /> or null if there is no such service.</returns>
    public static object? GetKeyedService(this IServiceProvider provider, Type serviceType, object? serviceKey)
    {
        return provider is IKeyedServiceProvider keyedServiceProvider
            ? keyedServiceProvider.GetKeyedService(serviceType, serviceKey)
            : throw new InvalidOperationException("Doesn't support keyed service provider");
    }
}