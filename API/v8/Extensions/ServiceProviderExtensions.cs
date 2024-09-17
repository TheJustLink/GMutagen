using System;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.IO;
using GMutagen.v8.Objects;
using GMutagen.v8.Contracts;

namespace GMutagen.v8.Extensions;

public static class ServiceProviderExtensions
{
    public static IReadWrite<TObjectId, ObjectValue<TContractId>> GetObjectValues<TObjectId, TContractId>(this IServiceProvider services)
        where TObjectId : notnull
    {
        return services.GetRequiredService<IReadWrite<TObjectId, ObjectValue<TContractId>>>();
    }
    public static IReadWrite<TContractId, ContractValue<TSlotId, TValueId>> GetContractValues<TContractId, TSlotId, TValueId>(this IServiceProvider services)
        where TContractId : notnull
        where TSlotId : notnull
    {
        return services.GetRequiredService<IReadWrite<TContractId, ContractValue<TSlotId, TValueId>>>();
    }
}