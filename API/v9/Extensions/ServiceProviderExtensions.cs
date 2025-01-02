using System;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData;
using GMutagen.v9.IO;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v9.Extensions;

public static class ServiceProviderExtensions
{
    public static IReadWrite<TObjectId, ObjectMetaData<TContractId>> GetObjectValues<TObjectId, TContractId>(this IServiceProvider services)
        where TObjectId : notnull
    {
        return services.GetRequiredService<IReadWrite<TObjectId, ObjectMetaData<TContractId>>>();
    }
    /*public static IReadWrite<TContractId, ContractValue<TSlotId, TValueId>> GetContractValues<TContractId, TSlotId, TValueId>(this IServiceProvider services)
        where TContractId : notnull
        where TSlotId : notnull
    {
        return services.GetRequiredService<IReadWrite<TContractId, ContractValue<TSlotId, TValueId>>>();
    }*/
}