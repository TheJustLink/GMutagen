using System;
using System.Collections.Generic;
using System.Linq;

using GMutagen.v6.Objects.Stubs;
using GMutagen.v6.Objects.Stubs.Implementation;
using GMutagen.v6.Objects.Template.Decsriptor;
using GMutagen.v6.Values;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GMutagen.v6.Objects.Template;

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
    private bool _addRelatedObjectsContract;

    public ObjectTemplate() : this(new HashSet<Type>(), new ServiceCollection(), false)
    {
    }

    public ObjectTemplate(bool addRelatedObjectsContract = false, params ObjectTemplate[] templates)
    {
        _addRelatedObjectsContract = addRelatedObjectsContract;
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

    private ObjectTemplate(HashSet<Type> contracts, ServiceCollection serviceCollection, bool addRelatedObjectsContract = false)
    {
        _contracts = contracts;
        _serviceCollection = serviceCollection;
        _addRelatedObjectsContract = addRelatedObjectsContract;
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

        if (_addRelatedObjectsContract)
            instanceContracts[typeof(RelatedObjectStorage)] = new ConstantContractStub(new RelatedObjectStorage(_objects));

        var instance = new Object(instanceContracts, this);
        return instance;
    }

    public ObjectTemplate AddFromGenerator<TInterface>(IGenerator<TInterface> generator) 
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), this, (_, _) => 
        {
            return generator.Generate();
        });
        _contracts.Add(typeof(TInterface));
        return this;
    }


    public FromObjectsSection AddFromObjectsSection(ObjectDesc[] objectDesc) =>
        new FromObjectsSection(objectDesc, _objects, _serviceCollection, this, _contracts);
    
    public ObjectTemplate AddFromObjects<TType>(params ObjectDesc[] objectDescs) =>
        AddFromObjects<TType, TType>(this, objectDescs);

    public ObjectTemplate AddFromObjects<TType>(ObjectTemplate key, params ObjectDesc[] objectDescs) =>
        AddFromObjects<TType, TType>(key, objectDescs);

    public ObjectTemplate AddFromObjects<TInterface, TType>(params ObjectDesc[] objectDescs) =>
        AddFromObjects<TInterface, TType>(this, objectDescs);

    public ObjectTemplate AddFromObjects<TInterface, TType>(ObjectTemplate key, params ObjectDesc[] objectDescs)
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface[]), key, (_, _) =>
        {
            var count = objectDescs.Length;
            var contracts = new TType[count];

            for (int i = 0; i < count; i++)
            {
                var objectDesc = objectDescs[i];

                if (_objects.TryGetValue(objectDesc, out var obj))
                    // ReSharper disable once NullableWarningSuppressionIsUsed
                    contracts[i] = obj.Get<TType>()!;
            }


            throw new Exception();
        });
        _contracts.Add(typeof(TInterface[]));

        return this;
    }

    public FromObjectSection AddFromObjectSection(ObjectDesc objectDesc) =>
         new FromObjectSection(objectDesc, _objects, _serviceCollection, this, _contracts);
    

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
        _contracts.Add(typeof(TInterface));

        return this;
    }


    public FromObjectSection AddObjectSection(ObjectTemplate template) => 
        AddObjectSection(template, out var desc);

    public FromObjectSection AddObjectSection(ObjectTemplate template, out ObjectDesc objectDesc)
    {
        objectDesc = new ObjectDesc(_id++);
        _pairs.Add(objectDesc, template);
        return new FromObjectSection(objectDesc, _objects, _serviceCollection, this, _contracts);
    }

    public FromObjectsSection AddObjectsSection(out ObjectDesc[] objectDescs, params ObjectTemplate[] templates) =>
         AddObjectsSection(out objectDescs, this, templates);

    public FromObjectsSection AddObjectsSection(params ObjectTemplate[] templates) =>
         AddObjectsSection(out _, this, templates);

    public FromObjectsSection AddObjectsSection(ObjectTemplate key, params ObjectTemplate[] templates) =>
         AddObjectsSection(out _, key, templates);

    public FromObjectsSection AddObjectsSection(out ObjectDesc[] objectDescs, ObjectTemplate key, params ObjectTemplate[] templates)
    {
        var count = templates.Length;
        objectDescs = new ObjectDesc[count];
        for (int i = 0; i < count; i++)
        {
            var template = templates[i];
            var objectDesc = new ObjectDesc(_id++);
            objectDescs[i] = objectDesc;

            _pairs.Add(objectDesc, template);
        }

        return new FromObjectsSection(objectDescs, _objects, _serviceCollection, key, _contracts);
    }

    public ObjectTemplate AddObject(ObjectTemplate template, out ObjectDesc objectDesc)
    {
        objectDesc = new ObjectDesc(_id++);
        _pairs.Add(objectDesc, template);
        return this;
    }
    
    public ObjectTemplate AddObjects(out ObjectDesc[] objectDescs, params ObjectTemplate[] templates)
    {
        var count = templates.Length;
        objectDescs = new ObjectDesc[count];
        for (int i = 0; i < count; i++)
        {
            var template = templates[i];
            var objectDesc = new ObjectDesc(_id++);
            objectDescs[i] = objectDesc;

            _pairs.Add(objectDesc, template);
        }

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
                    if (constructorAttribute is not InjectAttribute)
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
        _contracts.Add(typeof(TInterface));

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
        _contracts.Add(typeof(TInterface));
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
        _contracts.Add(typeof(TInterface));
        return this;
    }

    public ObjectTemplate Add<TInterface, TType>() where TType : class, TInterface where TInterface : class
    {
        _serviceCollection.AddKeyedTransient<TInterface, TType>(this);
        _contracts.Add(typeof(TInterface));
        return this;
    }

    public ObjectTemplate Add<TType>() where TType : class
    {
        _serviceCollection.AddKeyedTransient<TType>(this);
        _contracts.Add(typeof(TType));
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

public class FromObjectSection
{
    private readonly HashSet<Type> _contracts;
    private readonly IServiceCollection _serviceCollection;
    private readonly ObjectDesc _objectDesc;
    private readonly Dictionary<ObjectDesc, Object> _objects;
    private ObjectTemplate _key;

    public FromObjectSection(ObjectDesc objectDesc, Dictionary<ObjectDesc, Object> objects, IServiceCollection serviceCollection, ObjectTemplate key, HashSet<Type> contracts)
    {
        _objectDesc = objectDesc;
        _objects = objects;
        _serviceCollection = serviceCollection;
        _key = key;
        _contracts = contracts;
    }

    public FromObjectSection AddFromObject<TType>() =>
        AddFromObject<TType, TType>(_key);

    public FromObjectSection AddFromObject<TType>(ObjectTemplate key) =>
        AddFromObject<TType, TType>(key);

    public FromObjectSection AddFromObject<TInterface, TType>() =>
        AddFromObject<TInterface, TType>(_key);

    public FromObjectSection AddFromObject<TInterface, TType>(ObjectTemplate key)
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface), key, (_, _) =>
        {
            if (_objects.TryGetValue(_objectDesc, out var obj))
                // ReSharper disable once NullableWarningSuppressionIsUsed
                return obj.Get<TType>()!;

            throw new Exception();
        });
        _contracts.Add(typeof(TInterface));

        return this;
    }

    public ObjectTemplate End() => _key;
}

public class FromObjectsSection
{
    private readonly HashSet<Type> _contracts;
    private readonly IServiceCollection _serviceCollection;
    private readonly ObjectDesc[] _objectDescs;
    private readonly Dictionary<ObjectDesc, Object> _objects;
    private ObjectTemplate _key;

    public FromObjectsSection(ObjectDesc[] objectDescs, Dictionary<ObjectDesc, Object> objects, IServiceCollection serviceCollection, ObjectTemplate key, HashSet<Type> contracts)
    {
        _objectDescs = objectDescs;
        _objects = objects;
        _serviceCollection = serviceCollection;
        _key = key;
        _contracts = contracts;
    }

    public FromObjectsSection AddFromObjects<TType>() =>
        AddFromObjects<TType, TType>(_key);

    public FromObjectsSection AddFromObjects<TType>(ObjectTemplate key) =>
        AddFromObjects<TType, TType>(key);

    public FromObjectsSection AddFromObjects<TInterface, TType>() =>
        AddFromObjects<TInterface, TType>(_key);

    public FromObjectsSection AddFromObjects<TInterface, TType>(ObjectTemplate key)
    {
        _serviceCollection.AddKeyedTransient(typeof(TInterface[]), key, (_, _) =>
        {
            var count = _objectDescs.Length;
            var contracts = new TType[count];

            for (int i = 0; i < count; i++)
            {
                var objectDesc = _objectDescs[i];

                if (_objects.TryGetValue(objectDesc, out var obj))
                    // ReSharper disable once NullableWarningSuppressionIsUsed
                    contracts[i] = obj.Get<TType>()!;
            }


            throw new Exception();
        });
        _contracts.Add(typeof(TInterface[]));

        return this;
    }

    public ObjectTemplate End() => _key;
}

public class RelatedObjectStorage
{
    private readonly Dictionary<ObjectDesc, Object> _relatedObjects;

    public RelatedObjectStorage(Dictionary<ObjectDesc, Object> relatedObjects)
    {
        _relatedObjects = relatedObjects;
    }

    public Object this[ObjectDesc desc] => _relatedObjects[desc];
}

public static class DefaultGenerators
{
    public static Type DefaultIdType = typeof(int);

    public static IGenerator<IValue<TType>> GetExternalValueGenerator<TType>()
    {
        return GetExternalValueGenerator<TType>(typeof(TType), DefaultIdType);
    }

    public static IGenerator<object> GetExternalValueGenerator(Type targetType)
    {
        return GetExternalValueGenerator<object>(targetType, DefaultIdType);
    }

    public static IGenerator<IValue<TType>> GetExternalValueGenerator<TType>(Type targetType, Type idType)
    {
        var mapObj = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(idType, targetType));
        var memoryObj =
            Activator.CreateInstance(typeof(IO.Repositories.MemoryRepository<,>).MakeGenericType(idType, targetType), mapObj);
        var idGeneratorObj = Activator.CreateInstance(typeof(Id.IncrementalGenerator<>).MakeGenericType(idType));
        var generatorObj =
            Activator.CreateInstance(typeof(Values.ExternalValueGenerator<,>).MakeGenericType(idType, targetType),
                idGeneratorObj, memoryObj);
        var generator = (IGenerator<IValue<TType>>)generatorObj!;
        return generator;
    }
}

public class InjectAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class IdAttribute : Attribute
{
    public int Id;

    public IdAttribute(int id)
    {
        Id = id;
    }
}