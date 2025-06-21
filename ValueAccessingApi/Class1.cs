using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Values.Interfaces;
using Identity.Interfaces;
using Identity.Realizations;

namespace VariableAccessingApi;


public class ObjectValueStorage(Dictionary<ISingleId, IValue> translationMap) : IReadWrite<ISingleId, IValue>
{
    private readonly Dictionary<ISingleId, IValue> _translationMap = translationMap;

    public IValue Get(IId id)
    {
        if (id is not ISingleId singleId)
            return null!;

        return _translationMap[singleId];
    }
    
    public IValue Get<T>(IId id)
    {
        if (id is not ISingleId singleId)
            return null!;

        return (IValue<T>)_translationMap[singleId];
    }

    public IValue Get(ISingleId id)
        => _translationMap[id];
    
    public IValue<T> Get<T>(ISingleId id)
        => (IValue<T>)_translationMap[id];


    public bool TryGet(IId id, out IValue result)
    {
        result = null!;

        if (id is not ISingleId singleId)
            return false;

        return _translationMap.TryGetValue(singleId, out result!);
    }

    public bool TryGet(ISingleId id, out IValue result)
        => _translationMap.TryGetValue(id, out result!);

    public bool TryGet<T>(IId id, out IValue<T> result)
    {
        result = null!;

        if (id is not ISingleId singleId)
            return false;

        var isSuccess = _translationMap.TryGetValue(singleId, out var resultObj);

        if (!isSuccess)
            return false;

        result = (IValue<T>)resultObj!;
        return true;
    }

    public bool TryGet<T>(ISingleId id, out IValue<T> result)
    {
        result = null!;

        var isSuccess = _translationMap.TryGetValue(id, out var resultObj);

        if (!isSuccess)
            return false;

        result = (IValue<T>)resultObj!;
        return true;
    }

    public int Count => _translationMap.Count;

    public IValue this[ISingleId id]
    {
        get => _translationMap[id];
        set => _translationMap[id] = value;
    }

    public void Write(ISingleId id, IValue value)
        => Add(id, value);

    public IValue Read(ISingleId id)
        => Get(id);
    
    public IValue<T> Read<T>(ISingleId id)
        => Get<T>(id);

    public bool Contains(ISingleId id)
        => _translationMap.ContainsKey(id);
    
    public ObjectValueStorage Add(ISingleId semantic, IValue value)
    {
        _translationMap.Add(semantic, value);
        return this;
    }

    public ObjectValueStorage Remove(ISingleId semantic)
    {
        _translationMap.Remove(semantic);
        return this;
    }

    public ObjectValueStorage Clear()
    {
        _translationMap.Clear();
        return this;
    }

    public ObjectValueStorage Union(ObjectValueStorage objectValueStorage)
    {
        foreach (var pair in objectValueStorage._translationMap)
            _translationMap.Add(pair.Key, pair.Value);

        return this;
    }

    public ObjectValueStorage Union(Dictionary<ISingleId, IValue> translationMap)
    {
        foreach (var pair in translationMap)
            _translationMap.Add(pair.Key, pair.Value);

        return this;
    }
}

public class RegisterValueInValueStorage(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        var result = resolver.Resolve(context);

        if (!context.Type.IsAssignableTo(typeof(IValue)))
            return result;

        if (!context.TryGetOption<string>(OptionType.SemanticName, out var name))
            return result;

        var valueStorageContext = Context.From(context, typeof(ObjectValueStorage));
        var resolveValuesStorageResult = resolver.Resolve(valueStorageContext);

        if (!resolveValuesStorageResult)
            return result;

        var id = new Id<string>(name);
        var valueStorage = (ObjectValueStorage)valueStorageContext.Instance!;
        var value = (IValue)context.Instance!;
        valueStorage.Add(id, value);
        return result;
    }
}