using System;
using System.Collections.Generic;
using System.Reflection;

namespace GMutagen.v5;

public interface IContainer
{
    IAddContext Add<T>();
    T Resolve<T>();
    object Resolve(Type type);
}

public class ObjectTemplateContainer : IContainer
{
    public readonly Dictionary<Type, object> Dictionary;

    private readonly AddContext _addContext;
    public GeneratorsMap GeneratorsMap;

    public ObjectTemplateContainer()
    {
        Dictionary = new Dictionary<Type, object>();
        _addContext = new AddContext(this);
        GeneratorsMap = new GeneratorsMap();
    }

    public IAddContext Add<T>()
    {
        Dictionary.Add(typeof(T), null);
        _addContext.KeyType = typeof(T);
        return _addContext;
    }

    public T Resolve<T>()
    {
        var targetType = typeof(T);
        var instance = (T)Resolve(targetType);
        return instance;
    }

    private bool ValueExist(Type type, out object obj)
    {
        return Dictionary.TryGetValue(type, out obj) && obj != null;
    }

    private bool ContainsInstance(object obj)
    {
        return !(obj is Type);
    }

    public object Resolve(Type targetType)
    {
        if (!ValueExist(targetType, out var obj))
            throw new Exception("Value do not set yet");

        if (ContainsInstance(obj))
            return Dictionary[targetType];

        var objType = (Type)obj;
        var instance = Activator.CreateInstance(objType)!;
        foreach (var field in objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (TryInitFromGenerators(instance, objType, field))
                continue;

            var fieldType = field.FieldType;
            var fieldEntityInstance = Resolve(fieldType);
            field.SetValue(instance, fieldEntityInstance);
        }

        return instance;
    }

    private bool TryInitFromGenerators(object instance, Type instanceType, FieldInfo field)
    {
        if (GeneratorsMap == null)
            return false;

        IGenerator<object> generator;

        foreach (var attribute in field.GetCustomAttributes())
        {
            if (attribute is not IdAttribute id)
                continue;

            if (!GeneratorsMap.TryGetGenerator(instanceType, id.Id, out generator))
                break;

            field.SetValue(instance, generator.Generate());

            return true;
        }
        
        if (GeneratorsMap.TryGetGenerator(field, out generator))
        {
            field.SetValue(instance, generator.Generate());
            return true;
        }

        return false;
    }

    public object this[Type key]
    {
        get => Dictionary[key];
        set => Dictionary[key] = value;
    }
}

public class GeneratorsMap
{
    private readonly Dictionary<Type, Dictionary<int, IGenerator<object>>> _map;

    private Type _targetType;

    public GeneratorsMap()
    {
        _map = new Dictionary<Type, Dictionary<int, IGenerator<object>>>();
    }

    public GeneratorsMap SetTargetType(Type targetType)
    {
        _targetType = targetType;
        return this;
    }
    
    public GeneratorsMap Add(Type targetType, int id, IGenerator<object> generator)
    {
        if (_map.TryGetValue(_targetType, out var idMap))
            idMap.Add(id, generator);
        else
            _map.Add(_targetType, new Dictionary<int, IGenerator<object>> { { id, generator } });

        return this;
    }

    public GeneratorsMap Add(int id, IGenerator<object> generator)
    {
        if (_map.TryGetValue(_targetType, out var idMap))
            idMap.Add(id, generator);
        else
            _map.Add(_targetType, new Dictionary<int, IGenerator<object>> { { id, generator } });

        return this;
    }

    public GeneratorsMap AddDefault<TValue>(int id)
    {
        var generator = DefaultGenerators.GetExternalValueGenerator(typeof(TValue));

        if (_map.TryGetValue(_targetType, out var idMap))
            idMap.Add(id, generator);
        else
            _map.Add(_targetType, new Dictionary<int, IGenerator<object>> { { id, generator } });

        return this;
    }

    public bool TryGetGenerator(FieldInfo field, out IGenerator<object> generator)
    {
        if (typeof(IValue).IsAssignableFrom(field.FieldType))
        {
            generator = DefaultGenerators.GetExternalValueGenerator(field.FieldType.GenericTypeArguments[0]);
            return true;
        }

        generator = null!;
        return false;
    }

    public bool TryGetGenerator(Type instanceType, int id, out IGenerator<object> generator)
    {
        if (_map.TryGetValue(instanceType, out var idMap))
            return idMap.TryGetValue(id, out generator);

        generator = null!;
        return false;
    }
}

public static class DefaultGenerators
{
    public static Type DefaultIdType = typeof(int);

    public static IGenerator<object> GetExternalValueGenerator(Type targetType)
    {
        return GetExternalValueGenerator(targetType, DefaultIdType);
    }

    public static IGenerator<object> GetExternalValueGenerator(Type targetType, Type idType)
    {
        var mapObj = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(idType, targetType));
        var memoryObj =
            Activator.CreateInstance(typeof(MemoryRepository<,>).MakeGenericType(idType, targetType), mapObj);
        var idGeneratorObj = Activator.CreateInstance(typeof(IncrementalGenerator<>).MakeGenericType(idType));
        var generatorObj =
            Activator.CreateInstance(typeof(ExternalValueGenerator<,>).MakeGenericType(idType, targetType),
                idGeneratorObj, memoryObj);
        var generator = (IGenerator<object>)generatorObj!;
        return generator;
    }
}