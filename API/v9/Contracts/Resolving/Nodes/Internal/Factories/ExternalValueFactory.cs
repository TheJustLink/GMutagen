using GMutagen.v9.Contracts.Resolving.Nodes.Internal.Factories.Interfaces;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;
using GMutagen.v9.Values;

namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal.Factories;

public class ExternalValueFactory<TValueId, TValueType>(ILogger<Global> logger) : IValueFactory<TValueId>
{
    public object Create(TValueId id, object storageObj)
    {
        var storage = (IReadWrite<TValueId, TValueType>)storageObj;
        if (storage.Contains(id) is false)
            storage[id] = default!;
            
        var value = new ExternalValue<TValueId, TValueType>(id, storage);
        logger.LogInfo(new Created<ExternalValue<TValueId, TValueType>>(value, this));
        return value;
    }
}