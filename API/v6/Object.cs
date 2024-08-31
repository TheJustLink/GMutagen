using System;
using System.Collections.Generic;
using System.Linq;
using GMutagen.v5;
using GMutagen.v5.Container;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GMutagen.v6;

public class GeneratorDecorator<TIn, TOut> : IGenerator<TOut>
{
    private readonly IGenerator<TIn> _sourceGenerator;
    private readonly IGenerator<TOut, IGenerator<TIn>> _proxyGenerator;

    protected GeneratorDecorator(IGenerator<TIn> sourceGenerator, IGenerator<TOut, IGenerator<TIn>> proxyGenerator)
    {
        _sourceGenerator = sourceGenerator;
        _proxyGenerator = proxyGenerator;
    }

    public TOut Generate() => _proxyGenerator.Generate(_sourceGenerator);
}

public interface IGenerator<out T>
{
    T Generate();
}

public interface IValue
{
}

public abstract class ContractStub
{
    public abstract T Get<T>();
    public abstract bool TryGet<T>(out T contract);
}

public class ConstantFromObjectContractStub : ContractStub
{
    private readonly Object _obj;

    public ConstantFromObjectContractStub(Object obj)
    {
        _obj = obj;
    }

    public override T Get<T>()
    {
        return _obj.Get<T>();
    }

    public override bool TryGet<T>(out T contract)
    {
        return _obj.TryGet(out contract);
    }
}

public class ConstantContractStub : ContractStub
{
    private readonly object _contract;

    public ConstantContractStub(object contract)
    {
        _contract = contract;
    }

    public override T Get<T>()
    {
        return (T)_contract;
    }

    public override bool TryGet<T>(out T contract)
    {
        contract = (T)_contract;
        return true;
    }
}

public class Object : IObject
{
    private readonly Dictionary<Type, ContractStub> _staticContracts;
    // ReSharper disable once NotAccessedField.Local
    private readonly ObjectTemplate _template;

    public Object(Dictionary<Type, ContractStub> staticContracts, ObjectTemplate template)
    {
        _staticContracts = staticContracts;
        _template = template;
    }

    public T Get<T>()
    {
        return _staticContracts[typeof(T)].Get<T>();
    }

    public bool TryGet<T>(out T contract)
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        contract = default!;

        var success = _staticContracts.TryGetValue(typeof(T), out var contractStub);

        if (!success)
            return false;

        // ReSharper disable once NullableWarningSuppressionIsUsed
        if (contractStub!.TryGet(out contract))
            return true;

        return false;
    }
}

public class ObjectStateMachine : IObject
{
    private readonly Object[] _states;
    public int Index { get; set; }

    public ObjectStateMachine(Object[] states, int index)
    {
        _states = states;
        Index = index;
    }

    public T Get<T>()
    {
        return _states[Index].Get<T>();
    }

    public bool TryGet<T>(out T contract)
    {
        return _states[Index].TryGet(out contract);
    }
}

public class ObjectCompose : IObject
{
    private readonly Object[] _objects;

    public ObjectCompose(Object[] objects)
    {
        _objects = objects;
    }

    public T Get<T>()
    {
        foreach (var state in _objects)
        {
            if (state.TryGet<T>(out var contract))
                return contract;
        }

        throw new Exception();
    }

    public bool TryGet<T>(out T contract)
    {
        foreach (var state in _objects)
        {
            if (state.TryGet(out contract))
                return true;
        }

        // ReSharper disable once NullableWarningSuppressionIsUsed
        contract = default!;
        return false;
    }
}

public interface IObject
{
    T Get<T>();
    bool TryGet<T>(out T contract);
}

public class DynamicFromObjectContractStub<T> where T : class
{
    private readonly Object _targetObject;

    public DynamicFromObjectContractStub(Object targetObject)
    {
        _targetObject = targetObject;
    }

    public static implicit operator T(DynamicFromObjectContractStub<T> dynamicFromObjectContractStub)
    {
        if (dynamicFromObjectContractStub._targetObject.TryGet<T>(out var contract))
            return contract;

        // ReSharper disable once NullableWarningSuppressionIsUsed
        return null!;
    }
}

public interface IDynamicContract
{
    T Get<T>();
    bool TryGet<T>(out T contract);
}

public class FromObjectContractDynamic : IDynamicContract
{
    public Object Source { get; set; }

    public FromObjectContractDynamic(Object source)
    {
        Source = source;
    }

    public T Get<T>()
    {
        return Source.Get<T>();
    }

    public bool TryGet<T>(out T contract)
    {
        return Source.TryGet(out contract);
    }
}

public class FromObjectContractStatic : IDynamicContract
{
    private readonly Object _source;

    public FromObjectContractStatic(Object source)
    {
        _source = source;
    }

    public T Get<T>()
    {
        return _source.Get<T>();
    }

    public bool TryGet<T>(out T contract)
    {
        return _source.TryGet(out contract);
    }
}

public class ObjectTemplate
{
    private readonly HashSet<Type> _contracts;
    private readonly ServiceCollection _serviceCollection;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private IServiceProvider _serviceProvider = null!;
    // ReSharper disable once NullableWarningSuppressionIsUsed
    private Dictionary<ObjectDesc, ObjectTemplate> _pairs;
    private Dictionary<ObjectDesc, Object> _objects = null!;
    private int _id;

    public ObjectTemplate() : this(new HashSet<Type>(), new ServiceCollection())
    {
    }

    public ObjectTemplate(params ObjectTemplate[] templates)
    {
        _contracts = new HashSet<Type>();
        _serviceCollection = new ServiceCollection();

        foreach (var template in templates)
        {
            foreach (var contract in template._contracts)
                _contracts.Add(contract);

            foreach (var service in template._serviceCollection)
            {
                if (_serviceCollection.First(s => s.ServiceType == service.ServiceType) != null)
                    throw new ArgumentException("Can not resolve types");

                _serviceCollection.Add(service);
            }
        }
    }

    private ObjectTemplate(HashSet<Type> contracts, ServiceCollection serviceCollection)
    {
        _contracts = contracts;
        _serviceCollection = serviceCollection;
        _id = 0;
        _pairs = new Dictionary<ObjectDesc, ObjectTemplate>();
    }

    public Object Create()
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        _objects = new Dictionary<ObjectDesc, Object>();
        foreach (var pair in _pairs)
            _objects[pair.Key] = pair.Value.Create();
        
        // ReSharper disable once NullableWarningSuppressionIsUsed
        if (_serviceProvider == null!)
            _serviceProvider = _serviceCollection.BuildServiceProvider();

        var instanceContracts = new Dictionary<Type, ContractStub>();

        foreach (var contractType in _contracts)
        {
            var stub = new ConstantContractStub(_serviceProvider.GetRequiredKeyedService(contractType, this));
            instanceContracts.Add(contractType, stub);
        }

        var instance = new Object(instanceContracts, this);
        return instance;
    }

    public ObjectTemplate AddFromObject<TInterface, TType>(ObjectDesc objectDesc, ObjectTemplate key)
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), key, (s, u) =>
        {
            if (_objects.TryGetValue(objectDesc, out var obj))
                return obj.Get<TType>()!;

            throw new Exception();
        });

        return this;
    }

    public ObjectTemplate AddObject(ObjectTemplate template, out ObjectDesc objectDesc)
    {
        objectDesc = new ObjectDesc();
        _pairs.Add(objectDesc, template);
        return this;
    }

    public ObjectTemplate AddFromResolutionWithConstructor<TInterface, TType>()
        where TType : class, TInterface where TInterface : class
    {
        return AddFromResolutionWithConstructor<TInterface, TType>(this);
    }

    public ObjectTemplate AddFromResolutionWithConstructor<TInterface, TType>(ObjectTemplate key)
        where TType : class, TInterface where TInterface : class
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), key, (serviceProvider, _) =>
        {
            var instance = default(TType);
            foreach (var constructor in typeof(TType).GetConstructors())
            {
                foreach (var constructorAttribute in constructor.GetCustomAttributes(true))
                {
                    if (constructorAttribute is not Inject)
                        continue;

                    var parameters = constructor.GetParameters();
                    var resultParameters = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        var id = 0;
                        foreach (var parameterAttribute in parameter.GetCustomAttributes(true))
                        {
                            if (parameterAttribute is not IdAttribute idAttribute)
                                continue;

                            id = idAttribute.Id;
                        }

                        if (id == 0)
                            throw new Exception();

                        resultParameters[i] = serviceProvider.GetRequiredKeyedService(parameter.ParameterType, id);
                    }

                    // ReSharper disable once CoVariantArrayConversion
                    constructor.Invoke(instance, parameters);
                    // ReSharper disable once NullableWarningSuppressionIsUsed
                    return instance!;
                }
            }

            throw new Exception("Constructor with InjectAttribute was not found");
        });

        return this;
    }

    public ObjectTemplate AddFromAnotherKey<TInterface>(ObjectTemplate key) where TInterface : class
    {
        return AddFromAnotherKey<TInterface, TInterface>(key);
    }

    public ObjectTemplate AddFromAnotherKey<TInterface, TType>(ObjectTemplate key)
        where TType : class, TInterface where TInterface : class
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), this,
                                             (serviceProvider, _) =>
                                                 serviceProvider.GetRequiredKeyedService<TType>(key));
        return this;
    }

    public ObjectTemplate AddFromAnotherTemplate<TInterface>(ObjectTemplate template) where TInterface : class
    {
        return AddFromAnotherTemplate<TInterface>(template, this);
    }

    public ObjectTemplate AddFromAnotherTemplate<TInterface, TType>(ObjectTemplate template)
        where TType : class, TInterface where TInterface : class
    {
        return AddFromAnotherTemplate<TInterface, TType>(template, this);
    }

    public ObjectTemplate AddFromAnotherTemplate<TInterface>(ObjectTemplate template, ObjectTemplate key)
        where TInterface : class
    {
        return AddFromAnotherTemplate<TInterface, TInterface>(template, key);
    }

    public ObjectTemplate AddFromAnotherTemplate<TInterface, TType>(ObjectTemplate template, ObjectTemplate key)
        where TType : class, TInterface where TInterface : class
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), key, (_, _) => template.Create().Get<TType>());
        return this;
    }

    public ObjectTemplate Add<TInterface, TType>() where TType : class, TInterface where TInterface : class
    {
        _serviceCollection.AddKeyedTransient<TInterface, TType>(this);
        return this;
    }

    public ObjectTemplate Add<TType>() where TType : class
    {
        _serviceCollection.AddKeyedTransient<TType>(this);
        return this;
    }

    public ObjectTemplate Set<TInterface, TType>() where TType : class, TInterface where TInterface : class
    {
        for (int i = 0; i < _serviceCollection.Count; i++)
        {
            var service = _serviceCollection[i];
            if (service.ServiceType != typeof(TInterface))
                continue;

            var newService = new ServiceDescriptor(typeof(TInterface), service.ServiceKey, typeof(TType),
                                                   ServiceLifetime.Transient);

            _serviceCollection[i] = newService;
        }

        return this;
    }

    public ObjectTemplate Set<TInterface, TType>(ObjectTemplate key)
        where TType : class, TInterface where TInterface : class
    {
        for (int i = 0; i < _serviceCollection.Count; i++)
        {
            var service = _serviceCollection[i];
            if (service.ServiceType != typeof(TInterface) || service.ServiceKey != key)
                continue;

            var newService = new ServiceDescriptor(typeof(TInterface), service.ServiceKey, typeof(TType),
                                                   ServiceLifetime.Transient);

            _serviceCollection[i] = newService;
        }

        return this;
    }
}

public class Pair<T, T1>
{
    public ObjectDesc First { get; }
    public T1 Second { get; }

    public Pair(ObjectDesc first, T1 second)
    {
        First = first;
        Second = second;
    }
}

public class ObjectDesc
{
    public int Index;

    public ObjectDesc()
    {
        Index = int.MinValue;
    }

    public ObjectDesc(int index)
    {
        Index = index;
    }
}

public class IntValue : IValue<int>
{
    private int _value;

    public int Value
    {
        get => _value;
        set => _value = value;
    }
}