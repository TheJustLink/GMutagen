using System;
using Microsoft.Extensions.DependencyInjection;

using GMutagen.Objects;
using GMutagen.Contracts;
using GMutagen.IO;
using GMutagen.IO.Sources.Dictionary;

namespace GMutagen.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStoragesInDictionary<TObjectId, TContractId, TSlotId, TValueId>(this IServiceCollection services)
        where TObjectId : notnull
        where TContractId : notnull
        where TSlotId : notnull
        where TValueId : notnull
    {
        return services.AddStorages<TObjectId, TContractId, TSlotId, TValueId>(new DictionaryReadWriteFactory());
    }
    public static IServiceCollection AddStorages<TObjectId, TContractId, TSlotId, TValueId>(this IServiceCollection services, IReadWriteFactory storageFactory)
        where TObjectId : notnull
        where TContractId : notnull
        where TSlotId : notnull
        where TValueId : notnull
    {
        services.AddValues(storageFactory.CreateReadWrite<TValueId, object>);
        services.AddContracts(storageFactory.CreateReadWrite<TContractId, ContractValue<TSlotId, TValueId>>);
        services.AddObjects(storageFactory.CreateReadWrite<TObjectId, ObjectValue<TContractId>>);

        return services;
    }

    public static IServiceCollection AddObjects<TId, TContractId>(this IServiceCollection services, Func<IReadWrite<TId, ObjectValue<TContractId>>> storageFactory)
        where TId : notnull
    {
        return services.AddStorage(storageFactory);
    }
    public static IServiceCollection AddContracts<TId, TSlotId, TValueId>(this IServiceCollection services, Func<IReadWrite<TId, ContractValue<TSlotId, TValueId>>> storageFactory)
        where TId : notnull
        where TSlotId : notnull
    {
        return services.AddStorage(storageFactory);
    }
    public static IServiceCollection AddValues<TId>(this IServiceCollection services, Func<IReadWrite<TId, object>> storageFactory)
        where TId : notnull
    {
        return services.AddStorage(storageFactory);
    }

    public static IServiceCollection AddStorage<TId, TValue>(this IServiceCollection services, Func<IReadWrite<TId, TValue>> storageFactory)
        where TId : notnull
    {
        return services.AddSingleton(storageFactory());
    }
    public static IServiceCollection AddKeyedStorage<TKey, TId, TValue>(this IServiceCollection services, Func<IReadWrite<TId, TValue>> storageFactory)
        where TId : notnull
    {
        return services.AddKeyedSingleton<IReadWrite<TId, TValue>>(typeof(TKey), storageFactory());
    }
}