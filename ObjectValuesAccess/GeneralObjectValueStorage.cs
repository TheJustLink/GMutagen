using GMutagen.v8.Values;

namespace ObjectValuesAccess;

public class GeneralObjectValueStorage<TId> where TId : notnull
{
    private readonly Dictionary<Type, object> _storages;

    public GeneralObjectValueStorage(Dictionary<Type, object> storages)
    {
        _storages = storages;
    }
    
    public void Write<T>(TId id, IValue<T> value)
    {
        if(_storages.TryGetValue(typeof(T), out object result))
            ((ObjectValueStorage<TId, T>)result)[id] = value;
        
        var storage = new ObjectValueStorage<TId, T>();
        _storages[typeof(T)] = storage;
        storage[id] = value;
    }

    public IValue<T> Read<T>(TId id)
    {
        var storage = (ObjectValueStorage<TId, T>)_storages[typeof(T)];
        return storage[id];
    }
}