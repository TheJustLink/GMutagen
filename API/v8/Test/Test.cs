using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;

using GMutagen.v8.Id;
using GMutagen.v8.IO;
using GMutagen.v8.IO.Repositories;
using GMutagen.v8.Objects;
using GMutagen.v8.Objects.Template;
using GMutagen.v8.Values;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GMutagen.v8.Test;

class StorageSegment<TId>
{
    private readonly IStorage<TId> _storage;
    private readonly TId _id;

    public StorageSegment(IStorage<TId> storage, TId id)
    {
        _storage = storage;
        _id = id;
    }

    public TValue Read<TValue>() => _storage.Read<TValue>(_id);
    public void Write<TValue>(TValue value) => _storage.Write(_id, value);
}

class BucketStorage
{
    private readonly Dictionary<Type, List<object>> _bucketLists = new();

    public void Add<TId, TValue>(IReadWrite<TId, TValue> bucket)
    {
        if (_bucketLists.TryGetValue(typeof(TValue), out var buckets))
            buckets.Add(bucket);

        buckets = new List<object>();
        buckets.Add(bucket);

        _bucketLists.Add(typeof(TValue), buckets);
    }
    public IEnumerable<IReadWrite<TId, TValue>> GetBuckets<TId, TValue>()
    {
        return _bucketLists[typeof(TValue)].OfType<IReadWrite<TId, TValue>>();
    }
}
class ValueStorage<TId, TValue> : MemoryRepository<TId, IValue<TValue>>
{

}
class Storage<TId> : IStorage<TId>
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
interface IStorage<in TId> : IReadWrite<TId>
{
    IReadWrite<TId, TValue> GetBucket<TValue>();
}
interface IBucketFactory<in TId>
{
    IReadWrite<TId, T> Create<T>();
}
class BucketFactoryWithReplication<TId> : IBucketFactory<TId>
{
    private readonly IBucketFactory<TId> _sourceFactory;
    private readonly BucketStorage _bucketStorage;

    public BucketFactoryWithReplication(IBucketFactory<TId> sourceFactory, BucketStorage bucketStorage)
    {
        _sourceFactory = sourceFactory;
        _bucketStorage = bucketStorage;
    }
    public IReadWrite<TId, T> Create<T>()
    {
        var bucket = _sourceFactory.Create<T>();
        
        _bucketStorage.Add(bucket);

        return bucket;
    }
}
class MemoryBucketFactory<TId> : IBucketFactory<TId>
{
    public IReadWrite<TId, T> Create<T>() => new MemoryRepository<TId, T>();
}
class ValueLocationAttribute : Attribute { }
class InMemoryAttribute : ValueLocationAttribute { }
class InFileAttribute : ValueLocationAttribute { }

static class ServiceCollectionExtensions
{
    public static void AddDefaultMemoryStorage(this IServiceCollection services)
    {
        var bucketFactory = new MemoryBucketFactory<int>();

        services.AddDefaultStorage<int, InMemoryAttribute>(bucketFactory);
    }

    public static IServiceCollection AddDefaultStorage<TId, TKey>(this IServiceCollection services, IBucketFactory<TId> bucketFactory)
    {
        var storage = new Storage<TId>(bucketFactory);
        services.AddKeyedSingleton<IStorage<TId>>(typeof(TKey), storage);
        services.AddSingleton<IStorage<TId>>(storage);

        return services;
    }
    public static IServiceCollection AddStorage<TId, TKey>(this IServiceCollection services, IBucketFactory<TId> bucketFactory)
    {
        var storage = new Storage<TId>(bucketFactory);
        services.AddKeyedSingleton<IStorage<TId>>(typeof(TKey), storage);

        return services;
    }
}

// var a1 = new A(new FromDB());
// var a2 = new A(new FromMemory());
// var someTemplate = new ObjectTemplate();
//
// IMemoryValue<T> : IValue<T>
// ExternalValue<T> : IValue<T>
// FromMemory<T> : IValue<T>
// {
//      private T _value;
// }

//public class A
//{
//     private IValue<int> _b;
//
//     [Inject]
//     public class A([InDb]IValue<int> c, [InDb]IValue<int> b)
//     {
//          _b = b;
//          _c = c;
//     }
//      
//}
//
// someTemplate.AddTransient(typeof(IValue<int>), new CombineKey(new Id(0)), (provider, key) =>
// {
//      return provider.Get();
// })
// container.AddStorage(impl, typeof(InDB));

class ObjectTemplateBuilder<TId>
{
    private readonly ServiceCollection _buildServices = new();
    private readonly Dictionary<Type, object?> _contracts = new();

    public ObjectTemplateBuilder()
    {
    }

    public ObjectTemplateBuilder(ServiceCollection buildServices)
    {
        _buildServices.Add(buildServices);
    }

    public void Set<TContract, TImplementation>() where TContract : class
    {
        _buildServices.AddTransient<TContract>(provider =>
        {
            var type = typeof(TImplementation);
            var valueType = typeof(IValue<>);

            foreach (var constructor in type.GetConstructors())
            {
                if (constructor.GetCustomAttribute<InjectAttribute>() is null)
                    continue;

                var parameters = constructor.GetParameters();
                var resultParameters = new object[parameters.Length];

                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    if (parameter.ParameterType != valueType)
                    {
                        resultParameters[i] = provider.GetRequiredService(parameter.ParameterType);
                    }
                    else
                    {
                        var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
                        var storage = locationKey is not null
                            ? provider.GetRequiredKeyedService<IStorage<TId>>(locationKey)
                            : provider.GetRequiredService<IStorage<TId>>();

                        var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
                        var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TId), genericValueType);

                        Activator.CreateInstance(externalValueType, );
                    }
                }

                foreach (var parameter in constructor.GetParameters())
                {
                    
                }
                foreach (var constructorAttribute in constructor.GetCustomAttributes(true))
                {
                    if (constructorAttribute is not InjectAttribute)
                        continue;

                    var parameters = constructor.GetParameters();
                    var resultParameters = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                        resultParameters[i] = provider.GetRequiredService(parameters[i].ParameterType);

                    constructor.Invoke(instance, parameters);
                    return instance!;
                }
            }

            throw new Exception("Constructor with InjectAttribute was not found");
        });

        _contracts.Add(typeof(TContract), null);
    }
    public void Set<TContract>(TContract implementation) where TContract : class
    {
        _buildServices.AddSingleton<TContract>(implementation);
        _contracts.Add(typeof(TContract), implementation);
    }

    public ObjectTemplate Build()
    {
        var buildServices = _buildServices.BuildServiceProvider();

        foreach (var contract in _contracts)
        {
            if (contract.Value is not null) continue;

            var implementation = buildServices.GetRequiredService(contract.Key);
            _contracts[contract.Key] = implementation;
        }

        return new ObjectTemplate(_contracts!);
    }
}
class ObjectTemplate
{
    private readonly Dictionary<Type, object> _contracts;

    public ObjectTemplate(Dictionary<Type, object> contracts)
    {
        _contracts = contracts;
    }

    public TContract GetContract<TContract>() where TContract : class
    {
        return (TContract)_contracts[typeof(TContract)];
    }
}
class Object<TId> : IObject
{
    private readonly TId _id;
    private readonly ObjectTemplate _template;

    public Object(TId id, ObjectTemplate template)
    {
        _id = id;
        _template = template;
    }

    public TContract Get<TContract>() where TContract : class
    {
        return _template.GetContract<TContract>();
    }
}
interface IObject
{
    TContract Get<TContract>() where TContract : class;
}

class Test
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


        Game(new GuidGenerator());
        Game(new IncrementalGenerator<int>());
    }

    private static void Game<TId>(IGenerator<TId> idGenerator)
    {
        var abobaTemplate = new ObjectTemplate();
        var amogaTemplate = new ObjectTemplate()
            .AddObject(abobaTemplate, out var desc1)
            
            .AddFromObjectSection(desc1)
            .AddFromObject<IAmoga>()
            .AddFromObject<IAmoga>()
            .AddFromObject<IAmoga>()
            .End()
            
            .AddObject(abobaTemplate, out var desc2)
            .AddFromObject<IAmoga>(desc2);
        
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

internal class DefaultMoga : IAmoga
{
}

internal interface IAmoga
{
}