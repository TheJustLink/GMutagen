using System;
using System.Collections.Generic;
using System.Numerics;

namespace GMutagen.v3;


class Test
{
    public static void Main()
    {
        var vector2Repository = new MemoryValueRepository<Vector2>();
        var objectCreator = new ObjectCreator();

        var bulletTemplate = new ObjectTemplate();
        bulletTemplate.Add<IPosition>(new DefaultPosition(vector2Repository));

        
        var obj = objectCreator.Create(bulletTemplate);

        var position = obj.Get<IPosition>();
        var value = position.Value; // Id ?
    }
}

interface INextPosition : IPosition
{
}

interface IPosition : Object<Vector2>
{
}

class DefaultPosition : ObjectFromRepository<Vector2>, IPosition
{
    public DefaultPosition(MemoryValueRepository<Vector2> repository) : base(repository)
    {
    }

    public DefaultPosition(MemoryValueRepository<Vector2> repository, int index) : base(repository, index)
    {
    }
}

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


public interface Object<T>
{
    T Value { get; set; }
}

public class ValueUtil
{
    private static Dictionary<Type, ICloneable> _typeMap = new Dictionary<Type, ICloneable>
    {
        { typeof(Vector2), new Vector2Object() },
    };

    public ValueUtil()
    {
    }

    public ValueUtil(Dictionary<Type, ICloneable> typeMap)
    {
        _typeMap = typeMap;
    }

    public static Object<T> GetValue<T>()
    {
        var obj = _typeMap[typeof(T)];
        return (Object<T>)obj.Clone();
    }
}

public class ObjectFromRepository<T> : Object<T>
{
    protected int Index;
    protected readonly MemoryValueRepository<T> Repository;

    public ObjectFromRepository(MemoryValueRepository<T> repository)
    {
        Repository = repository;
        var value = ValueUtil.GetValue<T>();
        Repository.Add(value);
        Index = Repository.IndexOf(value);
    }

    public ObjectFromRepository(MemoryValueRepository<T> repository, int index)
    {
        Repository = repository;
        Index = index;
    }

    public MemoryValueRepository<T> GetRepository()
    {
        return Repository;
    }

    public int GetIndex()
    {
        return Index;
    }

    public void SetOtherIndex(int index)
    {
        Index = index;
    }

    public void SetOtherIndex(Object<T> @object)
    {
        Index = Repository.IndexOf(@object);
    }

    public T Value
    {
        get => Repository[Index].Value;
        set => Repository[Index].Value = value;
    }
}

public class Vector2Object : Object<Vector2>, ICloneable
{
    public Vector2 Value { get; set; }

    public object Clone()
    {
        return new Vector2Object();
    }
}

public class DirectObjectFromRepository<T> : ObjectFromRepository<T>
{
    private int _index;
    private readonly MemoryValueRepository<T> _repository;
    private Object<T> _directObject;

    public DirectObjectFromRepository(MemoryValueRepository<T> repository, int index) : base(repository, index)
    {
        _repository = repository;
        _index = index;
        _directObject = GetDirectValue(repository, index);
    }

    public DirectObjectFromRepository(MemoryValueRepository<T> repository, int index, Object<T> directObject) : base(
        repository,
        index)
    {
        _repository = repository;
        _index = index;
        _directObject = directObject;
    }

    private Object<T> GetDirectValue(MemoryValueRepository<T> repository, int index)
    {
        var value = repository[index];
        while (value is ObjectFromRepository<T> valueFromRepository)
        {
            index = valueFromRepository.GetIndex();
            repository = valueFromRepository.GetRepository();
            value = repository[index];
        }

        return value;
    }

    public void SetOtherIndex(int index)
    {
        _index = index;
        _directObject = GetDirectValue(_repository, _index);
    }

    public void SetOtherIndex(Object<T> @object)
    {
        _index = _repository.IndexOf(@object);
        _directObject = GetDirectValue(_repository, _index);
    }

    public T Value
    {
        get => _directObject.Value;
        set => _directObject.Value = value;
    }
}

public class MemoryValueRepository<T>
{
    private readonly List<Object<T>> _values;

    public MemoryValueRepository()
    {
        _values = new List<Object<T>>();
    }

    public MemoryValueRepository(List<Object<T>> values)
    {
        _values = values;
    }

    public Object<T> this[int index] => _values[index];

    public MemoryValueRepository<T> Add(Object<T> @object)
    {
        _values.Add(@object);
        return this;
    }

    public MemoryValueRepository<T> Remove(Object<T> @object)
    {
        _values.Remove(@object);
        return this;
    }

    public int IndexOf(Object<T> targetObject)
    {
        for (int i = 0; i < _values.Count; i++)
        {
            var value = _values[i];
            if (object.ReferenceEquals(value, targetObject))
                return i;
        }

        throw new ArgumentException("Value is not represent in repository " + targetObject);
    }
}