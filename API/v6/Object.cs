using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class FromObjectContractStub : ContractStub
{
    private readonly Object _obj;

    public FromObjectContractStub(Object obj)
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

public class StaticContractStub : ContractStub
{
    private readonly object _contract;

    public StaticContractStub(object contract)
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
        contract = default!;

        var success = _staticContracts.TryGetValue(typeof(T), out var contractStub);

        if (!success)
            return false;

        if (contractStub.TryGet(out contract))
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
        return _states[Index].TryGet<T>(out contract);
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
            if (state.TryGet<T>(out contract))
                return true;
        }

        contract = default!;
        return false;
    }
}


public interface IObject
{
    T Get<T>();
    bool TryGet<T>(out T contract);
}

public class DynamicContract<T> where T : class
{
    private readonly Object _targetObject;

    public DynamicContract(Object targetObject)
    {
        _targetObject = targetObject;
    }

    public static implicit operator T(DynamicContract<T> dynamicContract)
    {
        if (dynamicContract._targetObject.TryGet<T>(out var contract))
            return contract;
        
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
        return Source.TryGet<T>(out contract);
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
        return _source.TryGet<T>(out contract);
    }
}

public class ObjectTemplate
{
    private readonly HashSet<Type> _contracts;
    private readonly Dictionary<Type, object> _dynamicContractBindings;
    private readonly ServiceCollection _serviceCollection;
    private IServiceProvider _serviceProvider = null!;

    public ObjectTemplate() : this(new HashSet<Type>(), new ServiceCollection(), new Dictionary<Type, object>())
    {
    }

    public ObjectTemplate(params ObjectTemplate[] templates)
    {
        _dynamicContractBindings = new Dictionary<Type, object>();
        _contracts = new HashSet<Type>();
        _serviceCollection = new ServiceCollection();

        foreach (var template in templates)
        {
            foreach (var contract in template._contracts)
            {
                if (_contracts.Contains(contract))
                    continue;

                _contracts.Add(contract);
            }

            foreach (var service in template._serviceCollection)
            {
                if (_serviceCollection.First(s => s.ServiceType == service.ServiceType) != null)
                    throw new ArgumentException("Can not resolve types");

                _serviceCollection.Add(service);
            }

            foreach (var pair in template._dynamicContractBindings)
            {
                if(!_dynamicContractBindings.TryAdd(pair.Key, pair.Value))
                    throw new ArgumentException("Can not resolve types");
            }
        }
    }

    private ObjectTemplate(HashSet<Type> contracts, ServiceCollection serviceCollection, Dictionary<Type, object> dynamicContractBindings)
    {
        _contracts = contracts;
        _serviceCollection = serviceCollection;
        _dynamicContractBindings = dynamicContractBindings;
    }

    public Object Create()
    {
        if (_serviceProvider == null!)
            _serviceProvider = _serviceCollection.BuildServiceProvider();

        var instanceContracts = new Dictionary<Type, object>();

        foreach (var contractType in _contracts)
            instanceContracts.Add(contractType, _serviceProvider.GetRequiredService(contractType));

        foreach (var pair in _dynamicContractBindings)
        {
            var value = _dynamicContractBindings[pair.Key];
            
            switch (value)
            {
                case Object obj:
                    instanceContracts.Add(pair.Key, new FromObjectContractStatic(obj));
                    break;
                case ObjectTemplate objectTemplate:
                    instanceContracts.Add(pair.Key, new FromObjectContractStatic(objectTemplate.Create()));
                    break;
                case 
            }
        }

        var instance = new Object(instanceContracts, this);
        return instance;
    }

    public ObjectTemplate AddFromObject<TInterface>(Object obj) where TInterface : class
    {
        _dynamicContractBindings[typeof(TInterface)] = obj;
        return this;
    }
    public ObjectTemplate AddFromObject<TInterface>(FromObjectContractStatic binding) where TInterface : class
    {
        _dynamicContractBindings[typeof(TInterface)] = binding;
        return this;
    }
    public ObjectTemplate AddFromObject<TInterface>(FromObjectContractDynamic binding) where TInterface : class
    {
        _dynamicContractBindings[typeof(TInterface)] = binding;
        return this;
    }
    
    public ObjectTemplate AddFromObjectOf<TInterface>(ObjectTemplate template) where TInterface : class
    {
        _dynamicContractBindings[typeof(TInterface)] = template;
        return this;
    }
    
    public ObjectTemplate Add<TInterface, TType>() where TType : class, TInterface where TInterface : class
    {
        _serviceCollection.AddTransient<TInterface, TType>();
        return this;
    }

    public ObjectTemplate Add<TType>() where TType : class
    {
        _serviceCollection.AddTransient<TType>();
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
}