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
    public Dictionary<Type, Dictionary<int, IGenerator<object>>> TypeMap;

    public ObjectTemplateContainer()
    {
        Dictionary = new Dictionary<Type, object>();
        _addContext = new AddContext(this);
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
        return Dictionary.TryGetValue(type, out obj) && obj == null;
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

        var objType = obj.GetType();
        var instance = Activator.CreateInstance(objType)!;
        foreach (var field in objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (TryInitFromGenerators(obj, objType, field))
                continue;

            var fieldType = field.FieldType;
            var fieldEntityInstance = Resolve(fieldType);
            field.SetValue(instance, fieldEntityInstance);
        }

        return instance;
    }

    private bool TryInitFromGenerators(object instance, Type instanceType, FieldInfo field)
    {
        foreach (var attribute in field.GetCustomAttributes())
        {
            if (attribute is not IdAttribute id)
                continue;

            if (TypeMap == null ||
                !TypeMap.TryGetValue(instanceType, out var idMap) ||
                !idMap.TryGetValue(id.Id, out var generator))
                return false;

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