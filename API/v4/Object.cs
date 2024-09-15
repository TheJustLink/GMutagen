using System;
using System.Collections.Generic;
using System.Numerics;

namespace GMutagen.v4;

class Test
{
    public static void Main()
    {
        // var objectCreator = new ObjectCreator();

        // var bulletTemplate = new ObjectTemplate();
        // bulletTemplate.Add<IPosition>(new DefaultPosition());

        // var obj = objectCreator.CreateRead(bulletTemplate);

        // player
        // IPosition // IRead // IWrite

        //ObjectDesc desc = new ObjectDesc();

        //IRepository<object> objects; // IDictionary<int, T> where T : IList<int>

        // objects : Dictionary<ObjectDesc, object>
        {
            // player contracts : Dictionary<Type, ObjectDesc>
            {
                //   : ObjectDesc
                // IDirection : ObjectDesc

            }
            // InMemoryPosition : Value
            // InBDPosition : Value(?)
        }
        
        // Вариант с разделением объектов и контрактов
        // objects : Dictionary<ObjectDesc, Dictionary<Type, ContractDesc>>
        {
            // 0 : { IPosition : (3, 5) }
        }
        // contracts : Dictionary<type:int, Dictionary<id:int, value>>
        {
            // 3 : IRepo<Vector2>
            // 0 : IRepo<Vector2>
            // 1 : IRepo<int>
        }
        // Вариант с разделением объектов и контрактов v2
        // objects : IRepo<objectId, IRepo<Type, ContractDesc(contractId, valueId)>>
        {
            // 0 : { IPosition : (3, 5) }
        }
        // contracts : IRepo<contractId, IRepo<valueId, value>>
        {
            // 3 : IRepo<Vector2>
            // 0 : IRepo<Vector2>
            // 1 : IRepo<int>
        }

        // Совмещённый вариант
        // ContractDesc { contractId, valueId }
        // 
        // ObjectsRepo : IRepo
        // <
        // Id: objectId | contractId,
        // Value: IRepo<Type, ContractDesc> | IRepo<valueId, value>
        // >

        // Универсал 4x4 offroad двигатель 16 цилиндров, black niger цвет
        // ContractDesc { contractId, valueId }
        // 
        // ObjectsRepo : IRepo
        // <
        // Id: objectId | contractId,
        // Repo<Id, value>: IRepo<Type, ContractDesc> | IRepo<valueId, value>
        // >
        // Итого:
        // IRepo<Id, Value>,
        // где Id - это либо objectId, либо contractId
        // где Value - это IRepo<Id, Value>

        // desc.Set<IPosition>(????????????????);

        // var position = obj.CreateRead<IPosition>();
        // position.Set(0, "position"); // Id ?


        // var objects = new Repo<int, object>();
        //
        // var positionsRepo = new Repo<int, Vector3>();
        // objects[0] = positionsRepo;
        //
        // var playerObj = new Repo<Type, ContractDesc>();
        // playerObj[typeof(Position)] = new ContractDesc(0, 0);
        // playerObj[typeof(PreviousPosition)] = new ContractDesc(0, 1);
        // objects[1] = playerObj;

        // Set player previous position
        //var playerObjTest = objects[1] as IRepo<Type, ContractDesc>;
        //var playerPreviousPosition = playerObjTest[typeof(PreviousPosition)];
        //var positionsRepoTest = objects[playerPreviousPosition.Id] as IRepo<int, Vector3>;
        //positionsRepoTest[playerPreviousPosition.ValueId] = new Vector3(1, 2, 3);

        // Swap current player position and previous
        //var playerCurrentPosition = playerObjTest[typeof(Position)];
        //playerObjTest[typeof(Position)] = playerObjTest[typeof(PreviousPosition)];
        //playerObjTest[typeof(PreviousPosition)] = playerCurrentPosition;


        // ЛУЧШЕ НЕ СОВМЕЩАТЬ (ЗАЧЕМ?)
        // ContractDesc { contractId, valueId }
        // 
        // ObjectsRepo : IRepo<objectId, IRepo<Type, ContractDesc>>
        // ContractsRepo : IRepo<contractId, IRepo<valueId, value>>

        // ContractDesc { contractId, valueId }
        //var objects2 = new Repo<int, IRepo<Type, ContractDesc>>();
        //var contracts2 = new Repo<int, object>();

        //var objects3 = new Repo<int, IRepo<Type, int>>();
        //var contracts3 = new Repo<Type, object>();

        // var objects3 = new Repo<objectId, IRepo<contractId:Type, valueId>>();
        // var contracts3 = new Repo<contractType:Type, IRepo<valueId, value>>();
        //
        // objects[0][typeof(PreviousPosition)] = 2
        //
        // positions = contracts3.CreateRead<Vector3>()
        // positions[0] = new Vector3();
        // positions[0] = new Vector3();
        // positions[2] = new Vector3(); // previous position
        // 

        // ContractDesc { contractType, valueId }
        // var objects3 = new Repo<objectId, IRepo<contractId:Type, ContractDesc>>();
        // var contracts3 = new Repo<contractType:Type, IRepo<valueId, value>>();
        //
        // objects[0][typeof(Position)] = new(typeof(Vector2), 1)
        // objects[0][typeof(PreviousPosition)] = new(typeof(Vector2), 2)
        //
        // positions = contracts3.CreateRead<Vector2>()
        // positions[0] = new Vector3();
        // positions[1] = new Vector3(); // position
        // positions[2] = new Vector3(); // previous position
        // 

        // var objects4 = new Repo<int, Repo<Type, int>>();
        // var contracts4 = new Repo<Type, object>();

        // ContractDesc { contractType, valueId }
        // // var objects3 = new Repo<objectId, ContractDesc>();
        // // var contracts3 = new Repo<contractType:Type, IRepo<valueId, value>>();

        // var objects5 = new Repo<int, Repo<Type, object>>();


        // // var objects3 = new Repo<objectId, Dictionary<Type, IRepo<valueId, value>>>();
        // // var contracts3 = new Repo<contractType:Type, IRepo<valueId, value>>();

        var objects = new Repo<int, IRepo<Type, object>>();
        var objectCreator = new ObjectCreator(objects, new ObjectRepoInMemoryFactory());

        var playerTemplate = new ObjectTemplate();
        playerTemplate.Add<IPosition>(new DefaultPosition());

        
    }
}

public class ObjectRepoInMemoryFactory : IFactory<IRepo<Type, object>>
{
    public IRepo<Type, object> Create()
    {
        return new Repo<Type, object>();
    }
}
public class DefaultPosition : IPosition { }
public interface IPosition { }

public readonly struct Object
{
    public readonly int Id;

    private readonly IRepo<int, IRepo<Type, object>> _objects;

    public Object(int id, IRepo<int, IRepo<Type, object>> objects)
    {
        Id = id;
        _objects = objects;
    }

    public bool TryGet<T>(out T contract)
    {
        var success = _objects[Id].TryGet(typeof(T), out var value);
        contract = (T)value;
        return success;
    }
    public T Get<T>()
    {
        return (T)_objects[Id][typeof(T)];
    }

    public void Set<T>(T value)
    {
        _objects[Id][typeof(T)] = value;
    }
}
public class ObjectTemplate
{
    public readonly Dictionary<Type, object> Contracts = new();

    public IRepo<Type, object> ToRepo() 
    {
        return new Repo<Type, object>(Contracts);
    }

    public void AddEmpty<T>()
    {
        Contracts.Add(typeof(T), new EmptyContract());
    }
    public void Add<T>(T value)
    {
        Contracts.Add(typeof(T), value);
    }

    public void Set<T>(T value)
    {
        Contracts[typeof(T)] = value;
    }
}

public class EmptyContract { }

public class ObjectCreator
{
    private static int s_Id;
    private readonly IRepo<int, IRepo<Type, object>> _objects;
    private readonly IFactory<IRepo<Type, object>> _objectRepoFactory;

    public ObjectCreator(IRepo<int, IRepo<Type, object>> objects, IFactory<IRepo<Type, object>> objectRepoFactory)
    {
        _objects = objects;
        _objectRepoFactory = objectRepoFactory;
    }

    public Object Create(ObjectTemplate template)
    {
        var id = s_Id++;
        var objectRepo = _objectRepoFactory.Create();
        _objects[id] = objectRepo;

        foreach (var contract in template.Contracts)
            objectRepo[contract.Key] = contract.Value; //.Clone()

        return new Object(id, _objects);
    }
}
public interface IFactory<out T>
{
    T Create();
}


public class Repo<TId, TValue> : IRepo<TId, TValue>
{
    private readonly Dictionary<TId, TValue> _memory;

    public Repo() : this(new Dictionary<TId, TValue>())
    {
    }

    public Repo(Dictionary<TId, TValue> memory) 
    {
        _memory = memory;
    }

    public bool TryGet(TId id, out TValue value) => _memory.TryGetValue(id, out value);
    public TValue Get(TId id) => _memory[id];
    public void Set(TId id, TValue value) => _memory[id] = value;

    public TValue this[TId id]
    {
        get => Get(id);
        set => Set(id, value);
    }
}
public interface IRepo<in TId, TValue>
{
    bool TryGet(TId id, out TValue value);
    TValue Get(TId id) => this[id];
    void Set(TId id, TValue value) => this[id] = value;

    TValue this[TId id] { get; set; }
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
        Repository.Add(value);
        Index = Repository.IndexOf(value);
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

    public MemoryValueRepository<T> Add(IValue<T> value)
    {
        _values.Add(value);
        return this;
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