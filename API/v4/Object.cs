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

        // var obj = objectCreator.Create(bulletTemplate);

        // player
        // IPosition // IRead // IWrite

        ObjectDesc desc = new ObjectDesc();

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

        // var position = obj.Get<IPosition>();
        // position.Set(0, "position"); // Id ?


        var objects = new Repo<int, object>();
        
        var positionsRepo = new Repo<int, Vector3>();
        objects[0] = positionsRepo;

        var playerObj = new Repo<Type, ContractDesc>();
        playerObj[typeof(Position)] = new ContractDesc(0, 0);
        playerObj[typeof(PreviousPosition)] = new ContractDesc(0, 1);
        objects[1] = playerObj;

        // Set player previous position
        var playerObjTest = objects[1] as IRepo<Type, ContractDesc>;
        var playerPreviousPosition = playerObjTest[typeof(PreviousPosition)];
        var positionsRepoTest = objects[playerPreviousPosition.Id] as IRepo<int, Vector3>;
        positionsRepoTest[playerPreviousPosition.ValueId] = new Vector3(1, 2, 3);

        // Swap current player position and previous
        var playerCurrentPosition = playerObjTest[typeof(Position)];
        playerObjTest[typeof(Position)] = playerObjTest[typeof(PreviousPosition)];
        playerObjTest[typeof(PreviousPosition)] = playerCurrentPosition;


        // ЛУЧШЕ НЕ СОВМЕЩАТЬ (ЗАЧЕМ?)
        // ContractDesc { contractId, valueId }
        // 
        // ObjectsRepo : IRepo<objectId, IRepo<Type, ContractDesc>>
        // ContractsRepo : IRepo<contractId, IRepo<valueId, value>>

        // ContractDesc { contractId, valueId }
        var objects2 = new Repo<int, IRepo<Type, ContractDesc>>();
        var contracts2 = new Repo<int, object>();

        var objects3 = new Repo<int, IRepo<Type, int>>();
        var contracts3 = new Repo<Type, object>();

        // var objects3 = new Repo<objectId, IRepo<contractId:Type, valueId>>();
        // var contracts3 = new Repo<contractType:Type, IRepo<valueId, value>>();
        //
        // objects[0][typeof(PreviousPosition)] = 2
        //
        // positions = contracts3.Get<Vector3>()
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
        // positions = contracts3.Get<Vector2>()
        // positions[0] = new Vector3();
        // positions[1] = new Vector3(); // position
        // positions[2] = new Vector3(); // previous position
        // 

    }
}

public enum Position;
public enum PreviousPosition;
public struct ContractDesc
{
    public int Id;
    public int ValueId;

    public ContractDesc(int id, int valueId)
    {
        Id = id;
        ValueId = valueId;
    }
}
public class Repo<TId, TValue> : IRepo<TId, TValue>
{
    private readonly Dictionary<TId, TValue> _memory = new();

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
    TValue Get(TId id) => this[id];
    void Set(TId id, TValue value) => this[id] = value;

    TValue this[TId id] { get; set; }
}

public interface IRepository<T> : IRepositoryReader<T>, IRepositoryWriter<T>
{
    T Get(int id) => this[id];
    void Set(int id, T value) => this[id] = value;

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


public readonly struct ObjectDesc
{
    public readonly int Id;

    public ObjectDesc() 
    { 

    }

    public ObjectDesc(int id)
    {
        Id = id;
    }
}

public readonly struct LocalObjectDesc
{
    public readonly Dictionary<Type, object> _map;
}