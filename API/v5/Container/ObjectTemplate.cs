using System;
using System.Collections.Generic;
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