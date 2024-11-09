using GMutagen.v8.IO;
using GMutagen.v8.Values;

namespace ObjectValuesAccess;

public class ObjectValueStorage<TId, TValue> : IRead<TId, IValue<TValue>>, IWrite<TId, IValue<TValue>> where TId : notnull
{
    private readonly Dictionary<TId, IValue<TValue>> _values;

    public ObjectValueStorage() : this(new Dictionary<TId, IValue<TValue>>())
    {
        
    }
    
    public ObjectValueStorage(Dictionary<TId, IValue<TValue>> values)
    {
        _values = values;
    }
    
    public int Count => _values.Count;

    public IValue<TValue> this[TId id]
    {
        get => _values[id];
        set => _values[id] = value;
    }

    public void Write(TId id, IValue<TValue> value)
    {
        _values[id] = value;
    }

    public IValue<TValue> Read(TId id)
    {
        return _values[id];
    }

    public bool Contains(TId id)
    {
        return _values.ContainsKey(id);
    }
}