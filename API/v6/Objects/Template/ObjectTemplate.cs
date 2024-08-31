using System;
using System.Collections.Generic;
using System.Linq;
using GMutagen.v5;
using GMutagen.v5.Container;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GMutagen.v6;

public class ObjectTemplate
{
    private readonly HashSet<Type> _contracts;
    private readonly ServiceCollection _serviceCollection;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private IServiceProvider _serviceProvider = null!;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private readonly Dictionary<ObjectDesc, ObjectTemplate> _pairs;
    // ReSharper disable once NullableWarningSuppressionIsUsed
    private Dictionary<ObjectDesc, Object> _objects = null!;
    private int _id;

    public ObjectTemplate() : this(new HashSet<Type>(), new ServiceCollection())
    {
    }

    public ObjectTemplate(params ObjectTemplate[] templates)
    {
        _id = 0;
        _pairs = new Dictionary<ObjectDesc, ObjectTemplate>();
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

    public ObjectTemplate AddFromObject<TType>(ObjectDesc objectDesc) =>
        AddFromObject<TType, TType>(objectDesc, this);

    public ObjectTemplate AddFromObject<TType>(ObjectDesc objectDesc, ObjectTemplate key) =>
        AddFromObject<TType, TType>(objectDesc, key);

    public ObjectTemplate AddFromObject<TInterface, TType>(ObjectDesc objectDesc) =>
        AddFromObject<TInterface, TType>(objectDesc, this);
    

    public ObjectTemplate AddFromObject<TInterface, TType>(ObjectDesc objectDesc, ObjectTemplate key)
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), key, (_, _) =>
        {
            if (_objects.TryGetValue(objectDesc, out var obj))
                // ReSharper disable once NullableWarningSuppressionIsUsed
                return obj.Get<TType>()!;

            throw new Exception();
        });

        return this;
    }

    public ObjectTemplate AddObject(ObjectTemplate template, out ObjectDesc objectDesc)
    {
        objectDesc = new ObjectDesc(_id++);
        _pairs.Add(objectDesc, template);
        return this;
    }

    public ObjectTemplate AddFromResolutionWithConstructor<TInterface, TType>() 
        where TType : class, TInterface where TInterface : class =>
        AddFromResolutionWithConstructor<TInterface, TType>(this);
    

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

    public ObjectTemplate AddFromAnotherKey<TInterface>(ObjectTemplate key) where TInterface : class =>
        AddFromAnotherKey<TInterface, TInterface>(key);
    

    public ObjectTemplate AddFromAnotherKey<TInterface, TType>(ObjectTemplate key)
        where TType : class, TInterface where TInterface : class
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), this,
                                             (serviceProvider, _) =>
                                                 serviceProvider.GetRequiredKeyedService<TType>(key));
        return this;
    }

    public ObjectTemplate AddFromAnotherTemplate<TInterface>(ObjectTemplate template) where TInterface : class =>
        AddFromAnotherTemplate<TInterface>(template, this);
    

    public ObjectTemplate AddFromAnotherTemplate<TInterface, TType>(ObjectTemplate template)
        where TType : class, TInterface where TInterface : class =>
        AddFromAnotherTemplate<TInterface, TType>(template, this);
    

    public ObjectTemplate AddFromAnotherTemplate<TInterface>(ObjectTemplate template, ObjectTemplate key)
        where TInterface : class =>
        AddFromAnotherTemplate<TInterface, TInterface>(template, key);
    

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