using System;
using GMutagen.v9.Contracts;
using GMutagen.v9.IO;
using GMutagen.v9.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Extensions;

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