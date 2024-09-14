using System;
using System.Collections.Generic;

using GMutagen.v8.IO;
using GMutagen.v8.IO.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Test;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectsInMemory<TId, TSlotId>(this IServiceCollection services, Func<IReadWrite<Type, TSlotId>> factory)
    {
        return services.AddContractSlotsInMemory<TId, Type, TSlotId>(factory);
    }

    public static IServiceCollection AddContractSlotsInMemory<TId, TSlotId, TValue>(this IServiceCollection services, Func<IReadWrite<TSlotId, TValue>> factory)
    {
        return AddDeepStorage<TId, TSlotId, TValue>(services, factory);
    }
    public static IServiceCollection AddDeepStorage<TId, TDeepId, TValue>(this IServiceCollection services, Func<IReadWrite<TDeepId, TValue>> factory)
    {
        var dictionary = new Dictionary<TId, IReadWrite<TDeepId, TValue>>();
        
        IRead<TId, IReadWrite<TDeepId, TValue>> contractSlotsReader = new DictionaryRead<TId, IReadWrite<TDeepId, TValue>>(dictionary);
        IWrite<TId, IReadWrite<TDeepId, TValue>> contractSlotsWriter = new DictionaryWrite<TId, IReadWrite<TDeepId, TValue>>(dictionary);
        
        contractSlotsReader = new LazyRead<TId, IReadWrite<TDeepId, TValue>>(contractSlotsReader, contractSlotsWriter, factory);
        var contractSlotsReadWrite = new ReadWrite<TId, IReadWrite<TDeepId, TValue>>(contractSlotsReader, contractSlotsWriter);

        return services.AddSingleton<IReadWrite<TId, IReadWrite<TDeepId, TValue>>>(contractSlotsReadWrite);
    }

    public static IServiceCollection AddStorage<TId, TValue>(this IServiceCollection services, Func<IReadWrite<TId, TValue>> factory) 
    {
        return services.AddSingleton<IReadWrite<TId, TValue>>(factory());
    }
}