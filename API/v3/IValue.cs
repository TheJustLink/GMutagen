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
        bulletTemplate.Add<INextPosition>(new DefaultPosition(vector2Repository));


        var obj = objectCreator.Create(bulletTemplate);

        var position = obj.Get<IPosition>();
        var value = position.Value;
    }
}

interface INextPosition : IPosition
{
}

interface IPosition : IValue<Vector2>
{
}

class DefaultPosition : ValueFromRepository<Vector2>, INextPosition
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


public interface IValue<T>
{
    T Value { get; set; }
}

public class ValueUtil
{
    private static Dictionary<Type, ICloneable> _typeMap = new Dictionary<Type, ICloneable>
    {
        { typeof(Vector2), new Vector2Value() },
    };

    public ValueUtil()
    {
    }

    public ValueUtil(Dictionary<Type, ICloneable> typeMap)
    {
        _typeMap = typeMap;
    }

    public static IValue<T> GetValue<T>()
    {
        var obj = _typeMap[typeof(T)];
        return (IValue<T>)obj.Clone();
    }
}

public class ValueFromRepository<T> : IValue<T>
{
    protected int Index;
    protected readonly MemoryValueRepository<T> Repository;

    public ValueFromRepository(MemoryValueRepository<T> repository)
    {
        Repository = repository;
        var value = ValueUtil.GetValue<T>();
        Index = Repository.Add(value);
    }

    public ValueFromRepository(MemoryValueRepository<T> repository, int index)
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

    public void SetOtherIndex(IValue<T> obj)
    {
        Index = Repository.IndexOf(obj);
    }

    public T Value
    {
        get => Repository[Index].Value;
        set => Repository[Index].Value = value;
    }
}

public class Vector2Value : IValue<Vector2>, ICloneable
{
    public Vector2 Value { get; set; }

    public object Clone()
    {
        return new Vector2Value();
    }
}

public class DirectValueFromRepository<T> : ValueFromRepository<T>
{
    private int _index;
    private readonly MemoryValueRepository<T> _repository;
    private IValue<T> _directValue;

    public DirectValueFromRepository(MemoryValueRepository<T> repository, int index) : base(repository, index)
    {
        _repository = repository;
        _index = index;
        _directValue = GetDirectValue(repository, index);
    }

    public DirectValueFromRepository(MemoryValueRepository<T> repository, int index, IValue<T> directValue) : base(
        repository,
        index)
    {
        _repository = repository;
        _index = index;
        _directValue = directValue;
    }

    private IValue<T> GetDirectValue(MemoryValueRepository<T> repository, int index)
    {
        var value = repository[index];
        while (value is ValueFromRepository<T> valueFromRepository)
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
        _directValue = GetDirectValue(_repository, _index);
    }

    public void SetOtherIndex(IValue<T> value)
    {
        _index = _repository.IndexOf(value);
        _directValue = GetDirectValue(_repository, _index);
    }

    public T Value
    {
        get => _directValue.Value;
        set => _directValue.Value = value;
    }
}


public class MemoryValueRepository<T>
{
    private readonly List<IValue<T>> _values;

    public MemoryValueRepository()
    {
        _values = new List<IValue<T>>();
    }

    public MemoryValueRepository(List<IValue<T>> values)
    {
        _values = values;
    }

    public IValue<T> this[int index] => _values[index];

    public int Add(IValue<T> value)
    {
        _values.Add(value);
        return _values.Count - 1;
    }

    public MemoryValueRepository<T> Remove(IValue<T> value)
    {
        _values.Remove(value);
        return this;
    }

    public int IndexOf(IValue<T> targetValue)
    {
        for (int i = 0; i < _values.Count; i++)
        {
            var value = _values[i];
            if (object.ReferenceEquals(value, targetValue))
                return i;
        }

        throw new ArgumentException("Value is not represent in repository " + targetValue);
    }
}