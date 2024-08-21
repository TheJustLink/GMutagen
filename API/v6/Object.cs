using System;
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

public class Object
{
    private readonly Dictionary<Type, object> _staticContracts;
    private readonly Dictionary<Type, IDynamicContract> _dynamicContracts;
    private readonly ObjectTemplate _template;

    public Object(Dictionary<Type, object> staticContracts, ObjectTemplate template) : this(staticContracts, template, new Dictionary<Type, IDynamicContract>())
    {
    }

    public Object(Dictionary<Type, object> staticContracts, ObjectTemplate template, Dictionary<Type, IDynamicContract> dynamicContracts)
    {
        _staticContracts = staticContracts;
        _template = template;
        _dynamicContracts = dynamicContracts;
    }

    public T Get<T>()
    {
        if(_staticContracts.TryGetValue(typeof(T), out var contract))
            return (T)contract;

        if (_dynamicContracts.TryGetValue(typeof(T), out var dynamicContract))
            return dynamicContract.Get<T>();

        throw new Exception("Contract are not available");
    }

    public bool TryGet<T>(out T contract)
    {
        contract = default!;

        var success = _staticContracts.TryGetValue(typeof(T), out var contractObj);

        if (!success)
            return false;

        contract = (T)contractObj!;
        return true;
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