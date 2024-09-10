using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using GMutagen.v8.Id;
using GMutagen.v8.IO;
using GMutagen.v8.IO.Repositories;
using GMutagen.v8.Objects;
using GMutagen.v8.Objects.Template;
using GMutagen.v8.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GMutagen.v8.Test;

public class Storage<TId> : IStorage<TId>
{
    private readonly IBucketFactory<TId> _bucketFactory;

    private readonly Dictionary<Type, object> _buckets = new();

    public Storage(IBucketFactory<TId> bucketFactory)
    {
        _bucketFactory = bucketFactory;
    }

    public TValue Read<TValue>(TId id) => GetBucket<TValue>().Read(id);
    public void Write<TValue>(TId id, TValue value) => GetBucket<TValue>().Write(id, value);

    public IReadWrite<TId, T> GetBucket<T>()
    {
        if (_buckets.TryGetValue(typeof(T), out var cachedBucket))
            return (IReadWrite<TId, T>)cachedBucket;

        var newBucket = _bucketFactory.Create<T>();
        _buckets[typeof(T)] = newBucket;

        return newBucket;
    }
}

public interface IStorage<in TId> : IReadWrite<TId>
{
    IReadWrite<TId, TValue> GetBucket<TValue>();
}

public interface IBucketFactory<in TId>
{
    IReadWrite<TId, T> Create<T>();
}

public class MemoryBucketFactory<TId> : IBucketFactory<TId>
{
    public IReadWrite<TId, T> Create<T>() => new MemoryRepository<TId, T>();
}

public class ValueLocationAttribute : Attribute
{
}

public class InMemoryAttribute : ValueLocationAttribute
{
}

public class InFileAttribute : ValueLocationAttribute
{
}

public static class ServiceCollectionExtensions
{
    public static void AddDefaultMemoryStorage(this IServiceCollection services)
    {
        var bucketFactory = new MemoryBucketFactory<int>();

        services.AddDefaultStorage<int, InMemoryAttribute>(bucketFactory);
    }

    public static IServiceCollection AddDefaultStorage<TId, TKey>(this IServiceCollection services,
        IBucketFactory<TId> bucketFactory)
    {
        var storage = new Storage<TId>(bucketFactory);
        services.AddKeyedSingleton<IStorage<TId>>(typeof(TKey), storage);
        services.AddSingleton<IStorage<TId>>(storage);

        return services;
    }

    public static IServiceCollection AddStorage<TId, TKey>(this IServiceCollection services,
        IBucketFactory<TId> bucketFactory)
    {
        var storage = new Storage<TId>(bucketFactory);
        services.AddKeyedSingleton<IStorage<TId>>(typeof(TKey), storage);

        return services;
    }
}

public class ContractDescriptor
{
    public readonly Type Type;
    public readonly Type? ImplementationType;
    public readonly object? Implementation;

    public ContractDescriptor(Type type, Type? implementationType = null, object? implementation = null)
    {
        Type = type;
        ImplementationType = implementationType;
        Implementation = implementation;
    }

    public override int GetHashCode() => Type.GetHashCode();

    public static ContractDescriptor Create<TContract>() => new(typeof(TContract));

    public static ContractDescriptor Create<TContract, TImplementation>() =>
        new(typeof(TContract), typeof(TImplementation));

    public static ContractDescriptor Create<TContract>(object implementation) =>
        new(typeof(TContract), implementation.GetType(), implementation);
}

public class ObjectTemplateBuilder
{
    private readonly HashSet<ContractDescriptor> _contracts = new();

    public ObjectTemplateBuilder Add<TContract, TImplementation>()
        where TContract : class where TImplementation : TContract
    {
        _contracts.Add(ContractDescriptor.Create<TContract, TImplementation>());
        return this;
    }

    public ObjectTemplateBuilder Add<TContract>() where TContract : class
    {
        _contracts.Add(ContractDescriptor.Create<TContract>());
        return this;
    }

    public ObjectTemplateBuilder Add<TContract>(TContract implementation) where TContract : class
    {
        _contracts.Add(ContractDescriptor.Create<TContract>(implementation));
        return this;
    }

    public ObjectTemplate Build() => new(_contracts);
}

public class ObjectBuilder
{
    private IContractResolver _contractResolver;
    private IObjectFactory _objectFactory;
    private readonly Dictionary<Type, ContractDescriptor> _contracts = new();

    public ObjectBuilder(ObjectTemplate template)
    {
        foreach (var contract in template.Contracts)
            _contracts.Add(contract.Type, contract);
    }

    public ObjectBuilder SetResolver(IContractResolver contractResolver)
    {
        _contractResolver = contractResolver;
        return this;
    }

    public ObjectBuilder SetObjectFactory(IObjectFactory objectFactory)
    {
        _objectFactory = objectFactory;
        return this;
    }

    public ObjectBuilder Set<TContract, TImplementation>() where TContract : class
    {
        Set(ContractDescriptor.Create<TContract, TImplementation>());
        return this;
    }

    public ObjectBuilder Set<TContract>(TContract implementation) where TContract : class
    {
        Set(ContractDescriptor.Create<TContract>(implementation));
        return this;
    }

    public IObject Build()
    {
        var implementations = new Dictionary<Type, object>(_contracts.Count);

        foreach (var contract in _contracts.Values)
            implementations[contract.Type] = _contractResolver.Resolve(contract);

        return _objectFactory.Create(implementations);
    }

    private ObjectBuilder Set(ContractDescriptor contract)
    {
        if (_contracts.ContainsKey(contract.Type) is false)
            throw new ArgumentOutOfRangeException(nameof(contract.Type));

        _contracts[contract.Type] = contract;
        return this;
    }
}

public interface IObjectFactory
{
    IObject Create(Dictionary<Type, object> contracts);
}

public class DefaultObjectFactory<TId> : IObjectFactory
{
    private readonly IGenerator<TId> _idGenerator;

    public DefaultObjectFactory(IGenerator<TId> idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public IObject Create(Dictionary<Type, object> contracts)
    {
        var id = _idGenerator.Generate();
        return new Object<TId>(id, contracts);
    }
}

public interface IContractResolver
{
    object Resolve(ContractDescriptor contract);
}

public class ObjectTemplateConfiguration : IContractResolver
{
    private readonly ServiceCollection _serviceCollection = new();
    
    private object? _resolveKey = null;
    private ServiceProvider _serviceProvider = null!;
    
    public object Resolve(ContractDescriptor contract)
    {
        if (_serviceProvider == null!)
            _serviceProvider = _serviceCollection.BuildServiceProvider();
        
        if(_resolveKey != null)
            return _serviceProvider.GetRequiredKeyedService(contract.ImplementationType!, _resolveKey);
        
        return _serviceProvider.GetRequiredService(contract.ImplementationType!);

        // _buildServices.AddTransient<TContract>(provider =>
        // {
        //     var type = typeof(TImplementation);
        //     var valueType = typeof(IValue<>);
        //
        //     foreach (var constructor in type.GetConstructors())
        //     {
        //         if (constructor.GetCustomAttribute<InjectAttribute>() is null)
        //             continue;
        //
        //         var parameters = constructor.GetParameters();
        //         var resultParameters = new object[parameters.Length];
        //
        //         for (var i = 0; i < parameters.Length; i++)
        //         {
        //             var parameter = parameters[i];
        //
        //             if (parameter.ParameterType != valueType)
        //             {
        //                 resultParameters[i] = provider.GetRequiredService(parameter.ParameterType);
        //             }
        //             else
        //             {
        //                 var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
        //                 var storage = locationKey is not null
        //                 ? provider.GetRequiredKeyedService<IStorage<TId>>(locationKey)
        //                     : provider.GetRequiredService<IStorage<TId>>();
        //
        //                 var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
        //                 var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TId), genericValueType);
        //
        //                 Activator.CreateInstance(externalValueType, );
        //             }
        //         }
        //
        //         foreach (var parameter in constructor.GetParameters())
        //         {
        //
        //         }
        //         foreach (var constructorAttribute in constructor.GetCustomAttributes(true))
        //         {
        //             if (constructorAttribute is not InjectAttribute)
        //                 continue;
        //
        //             var parameters = constructor.GetParameters();
        //             var resultParameters = new object[parameters.Length];
        //             for (int i = 0; i < parameters.Length; i++)
        //                 resultParameters[i] = provider.GetRequiredService(parameters[i].ParameterType);
        //
        //             constructor.Invoke(instance, parameters);
        //             return instance!;
        //         }
        //     }
        //
        //     throw new Exception("Constructor with InjectAttribute was not found");
        // });

        //Ne amozh silno sinok
    }

    public ObjectTemplateConfiguration SetResolveKey(object? resolveKey)
    {
        _resolveKey = resolveKey;
        return this;
    }

    public ObjectTemplateConfiguration Add<TInterface, TImplementation>(object? key = null)
    {
        if (key == null)
            _serviceCollection.AddTransient(typeof(TInterface), ResolveFromProvider<TImplementation>);
        else
            _serviceCollection.AddKeyedTransient(typeof(TInterface), key, ResolveFromProvider<TImplementation>);
        
        return this;
    }

    public ObjectTemplateConfiguration AddResolution<TInterface, TImplementation>(object? key = null)
    {
        if (key == null)
            _serviceCollection.AddTransient(typeof(TInterface), typeof(TImplementation));
        else
            _serviceCollection.AddKeyedTransient(typeof(TInterface), key, typeof(TImplementation));

        return this;
    }

    public ObjectTemplateConfiguration ResolveFromAnotherKey<TType>(object? key, object? targetKey)
    {
        _serviceCollection.AddKeyedTransient(typeof(TType), key, ResolveFromAnotherKey<TType>(targetKey));
        return this;
    }

    private object ResolveFromProvider<TImplementation>(IServiceProvider serviceProvider)
    {
        var type = typeof(TImplementation);
        var valueType = typeof(IValue<>);
        var constructors = type.GetConstructors();
        
        foreach (var constructor in constructors)
        {
            if (constructor.GetCustomAttribute<InjectAttribute>() is null || constructors.Length != 1)
                continue;
        
            var parameters = constructor.GetParameters();
            var resultParameters = new object[parameters.Length];
        
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
        
                if (parameter.ParameterType != valueType)
                {
                    resultParameters[i] = serviceProvider.GetRequiredService(parameter.ParameterType);
                }
                else
                {
                    var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
                    var storage = locationKey is not null
                        ? serviceProvider.GetRequiredKeyedService<IStorage<TImplementation>>(locationKey)
                        : serviceProvider.GetRequiredService<IStorage<TImplementation>>();
        
                    var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
                    var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TImplementation), genericValueType,);
        
                    var instance = Activator.CreateInstance(externalValueType,);
                    constructor.Invoke(instance, resultParameters);
                    return instance!;
                }
            }
        }
        
        throw new Exception("Constructor with InjectAttribute was not found");
    }
    
    private object ResolveFromProvider<TImplementation>(IServiceProvider serviceProvider, object? key)
    {
        var type = typeof(TImplementation);
        var valueType = typeof(IValue<>);
        var constructors = type.GetConstructors();
        
        foreach (var constructor in constructors)
        {
            if (constructor.GetCustomAttribute<InjectAttribute>() is null || constructors.Length != 1)
                continue;
        
            var parameters = constructor.GetParameters();
            var resultParameters = new object[parameters.Length];
        
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
        
                if (parameter.ParameterType != valueType)
                {
                    resultParameters[i] = serviceProvider.GetRequiredKeyedService(parameter.ParameterType, key);
                }
                else
                {
                    var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
                    var storage = locationKey is not null
                    ? serviceProvider.GetRequiredKeyedService<IStorage<TImplementation>>(locationKey)
                        : serviceProvider.GetRequiredService<IStorage<TImplementation>>();
        
                    var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
                    var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TImplementation), genericValueType,);
        
                    var instance = Activator.CreateInstance(externalValueType,);
                    constructor.Invoke(instance, resultParameters);
                    return instance!;
                }
            }
        }
        
        throw new Exception("Constructor with InjectAttribute was not found");
    }
    
    private Func<IServiceProvider, object?, object?> ResolveFromAnotherKey<TType>(object? targetKey)
    {
        return ResolveFromProviderFromAnotherKey<TType>;
        
        object ResolveFromProviderFromAnotherKey<TImplementation>(IServiceProvider serviceProvider, object? key)
        {
            return serviceProvider.GetRequiredKeyedService(typeof(TImplementation), targetKey);
        }
    }
}

public class ObjectTemplate
{
    private readonly HashSet<ContractDescriptor> _contracts;

    public ObjectTemplate(HashSet<ContractDescriptor> contracts)
    {
        _contracts = contracts;
    }

    public IEnumerable<ContractDescriptor> Contracts => _contracts;
}

public class Object<TId> : IObject
{
    private readonly TId _id;
    private readonly Dictionary<Type, object> _contracts;

    public Object(TId id, Dictionary<Type, object> contracts)
    {
        _id = id;
        _contracts = contracts;
    }

    public TContract Get<TContract>() where TContract : class
    {
        return (TContract)_contracts[typeof(TContract)];
    }
}

public interface IObject
{
    TContract Get<TContract>() where TContract : class;
}

public interface ITestContract
{
}

public class TestContract : ITestContract
{
    private readonly IValue<int> _value1;
    private readonly IValue<int> _value2;

    public TestContract(IValue<int> value1, [InFile] IValue<int> value2)
    {
        _value1 = value1;
        _value2 = value2;
    }
}

public class Test
{
    public static void Main()
    {
        var gameConfig = new ServiceCollection();
        gameConfig.AddDefaultMemoryStorage();

        var gameServices = gameConfig.BuildServiceProvider();

        // Using

        var defaultStorage = gameServices.GetRequiredService<IStorage<int>>();
        var inMemoryStorage = gameServices.GetRequiredKeyedService<IStorage<int>>(typeof(InMemoryAttribute));

        var floatBucket = defaultStorage.GetBucket<float>();
        var externalValue = new ExternalValue<int, float>(0, floatBucket);

        externalValue.Value = 10;

        var snakeTemplateBuilder = new ObjectTemplateBuilder();
        snakeTemplateBuilder.Add<ITestContract, TestContract>();
        snakeTemplateBuilder.Add<ITestContract>();


        Game(new GuidGenerator());
        Game(new IncrementalGenerator<int>());
    }

    private static void Game<TId>(IGenerator<TId> idGenerator)
    {
        var templateBuilder = new ObjectTemplateBuilder()
            .Add<IPosition, DefaultPosition>();

        var template = templateBuilder.Build();

        var objectBuilder = new ObjectBuilder(template)
            .SetResolver(new ObjectTemplateConfiguration())
            .SetObjectFactory(new DefaultObjectFactory<int>(new IncrementalGenerator<int>()));


        var obj1 = objectBuilder.Build();
        var obj2 = objectBuilder.Build();
        var obj3 = objectBuilder.Build();

        var positionGenerator = GetDefaultPositionGenerator(idGenerator);

        var serviceCollection = new ServiceCollection()
            .AddSingleton(idGenerator)
            .AddScoped<IGenerator<IPosition, ITypeRead<IGenerator<object>>>, DefaultPositionGenerator>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        IGenerator<TId>? idGen1 = serviceProvider.GetService<IGenerator<TId>>();
        IGenerator<TId> idGen2 = serviceProvider.GetRequiredService<IGenerator<TId>>();

        using (var scope = serviceProvider.CreateScope())
        {
            // Unique position generator instance for this scope
            var scopedPositionGenerator = scope.ServiceProvider
                .GetRequiredService<IGenerator<IPosition, ITypeRead<IGenerator<object>>>>();
        }


        var playerTemplate = (dynamic)new object();
        // new Container.ObjectTemplate();
        playerTemplate.Add<IPosition>();

        var player = playerTemplate.Create();
        //player.Set<IPosition>(positionGenerator.Generate());
    }

    public static IGenerator<IPosition> GetDefaultPositionGenerator<TId>(IGenerator<TId> idGenerator)
    {
        IReadWrite<TId, Vector2> positions = new MemoryRepository<TId, Vector2>();
        IGenerator<IValue<Vector2>> positionValueGenerator =
            new ExternalValueGenerator<TId, Vector2>(idGenerator, positions);
        GeneratorOverGenerator<IValue<Vector2>> lazyPositionValueGenerator =
            new GeneratorOverGenerator<IValue<Vector2>>(positionValueGenerator, new LazyValueGenerator<Vector2>());

        ITypeReadWrite<IGenerator<object>>
            valueGenerators = new TypeRepository<IGenerator<object>>(); // fix it pls later pls pls pls
        valueGenerators.Write(lazyPositionValueGenerator);

        ITypeRead<IGenerator<object>> universalGenerator = valueGenerators;

        var positionGenerator = CreateContractGenerator(universalGenerator, new DefaultPositionGenerator());
        return positionGenerator;
    }

    public static IGenerator<TContract> CreateContractGenerator<TContract>(
        ITypeRead<IGenerator<object>> universalGenerator,
        IGenerator<TContract, ITypeRead<IGenerator<object>>> contractGenerator)
    {
        // return new GeneratorCache<TContract>(universalGenerator, contractGenerator);
        return null;
    }
}

public class DefaultMoga : IAmoga
{
}

public interface IAmoga
{
}