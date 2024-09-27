using System;

using Microsoft.Extensions.DependencyInjection;
using GMutagen.Objects;
using GMutagen.Contracts;
using GMutagen.IO;

namespace GMutagen.Extensions;

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