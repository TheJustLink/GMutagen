using System;
using System.Collections.Generic;

namespace GMutagen.v2;

class Test
{
    public static void Main()
    {
        var objectCreator = new ObjectCreator();
        
        var bulletTemplate = new ObjectTemplate();
        bulletTemplate.Add<IPosition>(new DefaultPosition());

        var obj = objectCreator.Create(bulletTemplate);

        var position = obj.Get<IPosition>();
        position.Set(0, "position"); // Id ?
    }
}

interface INextPosition : IPosition;
interface IPosition : IRepository<string>;
class DefaultPosition : MemoryRepository<string>, IPosition;

public abstract class Repository<T> : IRepository<T>
{
    public T this[int id]
    {
        get => Get(id);
        set => Set(id, value);
    }

    public abstract T Get(int id);
    public abstract void Set(int id, T value);
}
public interface IRepository<T> : IRepositoryReader<T>, IRepositoryWriter<T>
{
    T this[int id] { get; set; }
}
public interface IRepositoryReader<out T>
{
    T Get(int id);
}
public interface IRepositoryWriter<in T>
{
    void Set(int id, T value);
}
public class MemoryRepository<T> : Repository<T>
{
    private readonly Dictionary<int, T> _memory = new();

    public override T Get(int id) => _memory[id];
    public override void Set(int id, T value) => _memory[id] = value;
}

public class EmptyContract;

public readonly struct Object
{
    public readonly int Id;

    public Object(int id) => Id = id;

    public T Get<T>() => ObjectCreator.Instance.Get<T>(Id);
    public void Set<T>(T value) => ObjectCreator.Instance.Set<T>(Id, value);
}


public class ObjectTemplate
{
    public readonly List<KeyValuePair<Type, object>> Contracts = new();

    public void AddEmpty<T>()
    {
        AddInternal<T>(new EmptyContract());
    }
    public void Add<T>(T contract)
    {
        AddInternal<T>(contract);
    }
    private void AddInternal<T>(object contract)
    {
        Contracts.Add(new KeyValuePair<Type, object>(typeof(T), contract));
    }
}
public class ObjectCreator
{
    public static ObjectCreator Instance = null!;

    private static int s_id;

    private readonly MemoryRepository<Dictionary<Type, int>> _objectsMap = new();
    private readonly MemoryRepository<object> _contractsMap = new();
    //private MemoryRepository<object> _contracts = new();

    public ObjectCreator()
    {
        Instance = this;
    }

    public Object Create(ObjectTemplate template)
    {
        var objectId = s_id++;

        _objectsMap[objectId] = new Dictionary<Type, int>();

        foreach (var contract in template.Contracts)
        {
            var contractId = s_id++;
            
            _contractsMap[contractId] = contract.Value;
            _objectsMap[objectId].Add(contract.Key, contractId);
        }

        return new Object(objectId);
    }

    public T Get<T>(int objectId)
    {
        var contracts = _objectsMap.Get(objectId);
        var contractId = contracts[typeof(T)];

        return (T)_contractsMap[contractId];
    }
    public void Set<T>(int objectId, T contract)
    {
        var contracts = _objectsMap.Get(objectId);
        var contractId = contracts[typeof(T)];

        _contractsMap[contractId] = contract;
    }
}
