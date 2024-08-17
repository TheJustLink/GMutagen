using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace GMutagen.v5.Container;

public interface IContainer
{
    IAddContext Add<T>();
    T Resolve<T>();
    object Resolve(Type type);
}

public class ObjectTemplateContainer : IContainer
{
    public readonly Dictionary<Type, Bindings> Dictionary;

    private readonly AddContext _addContext;

    public ObjectTemplateContainer()
    {
        Dictionary = new Dictionary<Type, Bindings>();
        _addContext = new AddContext(this);
    }

    public IAddContext Add<T>()
    {
        return Add(typeof(T));
    }
    
    public IAddContext Add(Type targetType)
    {
        Dictionary.Add(targetType, new Bindings());
        _addContext.KeyType = targetType;
        return _addContext;
    }

    public T Resolve<T>()
    {
        var targetType = typeof(T);
        var instance = (T)Resolve(targetType);
        return instance;
    }

    public object Resolve(Type targetType)
    {
        if (!ValueExist(targetType, out var bindings))
            throw new Exception("Value do not set yet");

        object result;

        if (TryCacheBindingsOption(bindings, out result))
            return result;

        if (TryGeneratorBindingsOption(bindings, out result))
            return result;

        if (TryGeneratorConstructorBindingsOption(bindings, out result))
            return result;

        if (TryReflectionBindingsOption(bindings, out result))
            return result;

        throw new Exception($"Type: {targetType} was not binded");
    }

    private bool TryReflectionBindingsOption(Bindings bindings, out object result)
    {
        if (!bindings.TryGet<ReflectionBindingsOption>(OptionType.ResolveFrom, out var reflectionBindingsOption))
        {
            result = null!;
            return false;
        }

        var objType = reflectionBindingsOption!.TargetType;

        var constructor = GetConstructor(objType);

        var declaredParameters = constructor.GetParameters();
        var parameters = new object[declaredParameters.Length];

        for (var i = 0; i < declaredParameters.Length; i++)
        {
            var parameter = declaredParameters[i];

            var parameterType = parameter.ParameterType;
            var parameterEntityInstance = Resolve(parameterType);
            parameters[i] = parameterEntityInstance;
        }

        result = Activator.CreateInstance(objType, parameters)!;

        return true;
    }

    private bool TryGeneratorConstructorBindingsOption(Bindings bindings, out object result)
    {
        if (!bindings.TryGet<GeneratorConstructorBindings>(OptionType.ResolveFrom, out var generatorConstructorBindings))
        {
            result = null!;
            return false;
        }

        var generators = generatorConstructorBindings!.Generators;
        var parameters = new object[generators.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = generators[i].Generate();
        }

        result = Activator.CreateInstance(generatorConstructorBindings.TargetType, parameters);
        return true;
    }

    private ConstructorInfo GetConstructor(Type objType)
    {
        var constructors = objType.GetConstructors();

        if (constructors.Length == 1)
            return constructors[0];

        foreach (var constructorInfo in constructors)
        {
            foreach (var attribute in constructorInfo.GetCustomAttributes())
            {
                if (attribute.GetType() == typeof(Inject))
                    return constructorInfo;
            }
        }

        throw new Exception("Constructor cannot be found");
    }

    private bool TryGeneratorBindingsOption(Bindings bindings, out object result)
    {
        if (bindings.TryGet<GeneratorBindingsOption>(OptionType.ResolveFrom, out var generatorBindings))
        {
            result = generatorBindings!.Generator.Generate();
            return true;
        }

        result = null!;
        return false;
    }

    private bool ValueExist(Type type, out Bindings obj)
    {
        return Dictionary.TryGetValue(type, out obj!);
    }

    private bool TryCacheBindingsOption(Bindings bindings, out object result)
    {
        if (bindings.TryGet<CacheBindingsOption>(OptionType.Cache, out var cacheBindingOption))
        {
            result = cacheBindingOption!.Instance;
            return true;
        }

        result = null!;
        return false;
    }

    public Bindings this[Type key]
    {
        get => Dictionary[key];
        set => Dictionary[key] = value;
    }
}

public class Inject : Attribute
{
}

public class Bindings
{
    private readonly Dictionary<OptionType, BindingOption> _options;

    public Bindings() : this(new Dictionary<OptionType, BindingOption>())
    {
    }

    public Bindings(Dictionary<OptionType, BindingOption> options)
    {
        _options = options;
    }

    public void Set(OptionType optionType, BindingOption option)
    {
        _options[optionType] = option;
    }
    
    public void Remove(OptionType optionType)
    {
        _options.Remove(optionType);
    }

    public bool TryGet<T>(OptionType optionType, out T? option) where T : BindingOption
    {
        option = null;
        
        if (!_options.TryGetValue(optionType, out var optionObj))
            return false;

        if (optionObj is not T ToptionObj) 
            return false;
        
        option = ToptionObj;
        return true;
    }
}

public enum OptionType
{
    Cache,
    ResolveFrom,
}

public class BindingOption
{
}

public class CacheBindingsOption : BindingOption
{
    public CacheBindingsOption(object instance)
    {
        Instance = instance;
    }
    public object Instance { get; }
}

public class InstanceBindingsOption : BindingOption
{
    public InstanceBindingsOption(object instance)
    {
        Instance = instance;
    }

    public object Instance { get; }
}

public class ReflectionBindingsOption : BindingOption
{
    public ReflectionBindingsOption(Type targetType)
    {
        TargetType = targetType;
    }

    public Type TargetType { get; }
}

public class GeneratorBindingsOption : BindingOption
{
    public GeneratorBindingsOption(IGenerator<object> generator)
    {
        Generator = generator;
    }

    public IGenerator<object> Generator { get; set; }
}

public class GeneratorConstructorBindings : BindingOption
{
    public GeneratorConstructorBindings(Type targetType, params IGenerator<object>[] generators)
    {
        Generators = generators;
        TargetType = targetType;
    }

    public IGenerator<object>[] Generators { get; }
    public Type TargetType { get; }
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