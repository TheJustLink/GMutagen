using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GMutagen.v5;

public class ObjectTemplate
{
    private static Dictionary<Type, IGenerator<object>> _contractGenerators = new();

    private readonly Dictionary<Type, object> _contracts = new();

    public Object Create(Dictionary<int, IGenerator<object>> generators = null)
    {
        var instanceContracts = new Dictionary<Type, object>();

        foreach (var pair in _contracts)
        {
            if (pair.Value is IGenerator<object> clonable)
            {
                instanceContracts.Add(pair.Key, clonable.Generate());
            }
            else if (pair.Value is IFromReflection)
            {
                var type = pair.Value.GetType();
                var instance = Activator.CreateInstance(type);
                var fields = type.GetFields();
                foreach (var field in fields)
                {
                    if (field.FieldType.IsSubclassOf(typeof(IValue)))
                        field.SetValue(instance, GetValueFor(field, generators));
                }

                instanceContracts.Add(pair.Key, instance);
            }
            else instanceContracts.Add(pair.Key, pair.Value);
        }

        return new Object(instanceContracts);
    }

    private object GetValueFor(FieldInfo field, Dictionary<int, IGenerator<object>> generators)
    {
        foreach (var attribute in field.GetCustomAttributes())
        {
            if (attribute is IdAttribute idAttribute)
                return generators[idAttribute.Id].Generate();
        }

        throw new ArgumentException("Key was not represent in generators");
    }

    public void AddEmpty<T>()
    {
        _contracts.Add(typeof(T), new EmptyContract());
    }

    public void Add<T>(T value)
    {
        _contracts.Add(typeof(T), value);
    }

    public void Set<T>(T value)
    {
        _contracts[typeof(T)] = value;
    }
}

public class ObjectTemplateContainer : IContainer
{
    private Dictionary<Type, object> _dictionary;

    private AddContext _addContext;

    public ObjectTemplateContainer()
    {
        _dictionary = new Dictionary<Type, object>();
        _addContext = new AddContext(this);
    }

    public IAddContext Add<T>()
    {
        _dictionary.Add(typeof(T), null);
        ((ContainerContext)_addContext).KeyType = typeof(T);
        return _addContext;
    }

    public T Resolve<T>()
    {
        var type = typeof(T);
        if (_dictionary.TryGetValue(type, out var obj) && !(obj is Type))
            return (T)_dictionary[type];

        var objType = (Type)obj;
        var instance = (T)Activator.CreateInstance(objType);
        foreach (var field in objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            var fieldType = field.FieldType;
            var fieldEntityInstance = Resolve(fieldType);
            field.SetValue(instance, fieldEntityInstance);
        }

        return instance;
    }

    public object Resolve(Type type)
    {
        if (_dictionary.ContainsKey(type))
            return _dictionary[type];

        var instance = Activator.CreateInstance(type);
        foreach (var field in type.GetFields())
        {
            var fieldType = field.FieldType;
            var fieldEntityInstance = Resolve(fieldType);
            field.SetValue(instance, fieldEntityInstance);
        }

        return instance;
    }

    public object this[Type key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }
}

public interface IContainer
{
    IAddContext Add<T>();
    T Resolve<T>();
    object Resolve(Type type);
}

public abstract class ContainerContext
{
    public Type KeyType { get; set; }

    protected readonly ObjectTemplateContainer Container;

    protected ContainerContext(ObjectTemplateContainer container)
    {
        Container = container;
    }
}

public class AddContext : ContainerContext, IAddContext
{
    private readonly AddAsContext _addAsContext;
    public Type Type { get; set; }

    public AddContext(ObjectTemplateContainer container) : base(container)
    {
        _addAsContext = new AddAsContext(container);
    }

    public IAddAsContext As<T>()
    {
        var type = typeof(T);
        Container[KeyType] = type;
        _addAsContext.KeyType = KeyType;
        _addAsContext.Type = type;
        return _addAsContext;
    }

    public IContainer FromInstance(object instance)
    {
        var instanceType = instance.GetType();

        if (Type == null && !instanceType.IsSubclassOf(KeyType) && !KeyType.IsAssignableFrom(instanceType))
            throw new Exception("Is not instance of key type");

        if (Type != null && instance.GetType() != Type)
            throw new Exception("Types are not the same");

        Container[KeyType] = instance;
        return Container;
    }
}

public interface IAddContext
{
    IAddAsContext As<T>();
    IContainer FromInstance(object instance);
}

public interface IAddAsContext
{
    IContainer FromInstance(object instance);
}

public class AddAsContext : ContainerContext, IAddAsContext
{
    public Type Type { get; set; }

    public AddAsContext(ObjectTemplateContainer container) : base(container)
    {
    }

    public IContainer FromInstance(object instance)
    {
        if (instance.GetType() != Type)
            throw new Exception("Types are not the same");

        Container[KeyType] = instance;
        return Container;
    }
}