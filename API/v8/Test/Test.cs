using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.Id;
using GMutagen.v8.IO;
using GMutagen.v8.IO.Repositories;
using GMutagen.v8.Objects;
using GMutagen.v8.Objects.Template;
using GMutagen.v8.Values;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;
using System.Data.Common;

namespace GMutagen.v8.Test;

/// <summary>
/// Extension methods for getting services from an <see cref="T:System.IServiceProvider" />.
/// </summary>
public static class ServiceProviderKeyedServiceExtensions
{
    /// <summary>
    /// Get service of type <typeparamref name="T" /> from the <see cref="T:System.IServiceProvider" />.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="provider">The <see cref="T:System.IServiceProvider" /> to retrieve the service object from.</param>
    /// <param name="serviceKey">An object that specifies the key of service object to get.</param>
    /// <returns>A service object of type <typeparamref name="T" /> or null if there is no such service.</returns>
    public static object? GetKeyedService(this IServiceProvider provider, Type serviceType, object? serviceKey)
    {
        return provider is IKeyedServiceProvider keyedServiceProvider
            ? keyedServiceProvider.GetKeyedService(serviceType, serviceKey)
            : throw new InvalidOperationException("Doesn't support keyed service provider");
    }
}
public static class CustomAttributeDataExtensions
{
    public static bool Contains<T>(this IEnumerable<CustomAttributeData> attributes)
    {
        foreach (var attribute in attributes) 
        {
            if(attribute.AttributeType.IsAssignableTo(typeof(T)))
                return true;
        }

        return false;
    }

    public static CustomAttributeData? Get<T>(this IEnumerable<CustomAttributeData> attributes)
    {
        foreach (var attribute in attributes)
        {
            if (attribute.AttributeType.IsAssignableTo(typeof(T)))
                return attribute;
        }

        return null;
    }
}

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
    public object GetBucket(Type valueType)
    {
        if (_buckets.TryGetValue(valueType, out var cachedBucket))
            return cachedBucket;

        var newBucket = _bucketFactory.Create(valueType);
        _buckets[valueType] = newBucket;

        return newBucket;
    }
}

public interface IStorage<in TId> : IReadWrite<TId>
{
    IReadWrite<TId, TValue> GetBucket<TValue>();
    object GetBucket(Type valueType);
}

public interface IBucketFactory<in TId>
{
    IReadWrite<TId, T> Create<T>();
    object Create(Type valueType);
}
public class MemoryBucketFactory<TId> : IBucketFactory<TId> where TId : notnull
{
    public IReadWrite<TId, T> Create<T>() => new MemoryRepository<TId, T>();
    public object Create(Type valueType)
    {
        var memoryOpenType = typeof(MemoryRepository<,>);
        var memoryClosedType = memoryOpenType.MakeGenericType(typeof(TId), valueType);
        
        return Activator.CreateInstance(memoryClosedType)!;
    }
}
public class InMemoryAttribute : ValueLocationAttribute { }

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultMemoryStorage<TId>(this IServiceCollection services)
    {
        var bucketFactory = new MemoryBucketFactory<TId>();

        return services.AddDefaultStorage<TId, InMemoryAttribute>(bucketFactory);
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

    public ObjectTemplate Build() => new(_contracts);

    public ObjectTemplateBuilder Add<TContract, TImplementation>()
        where TContract : class where TImplementation : TContract
    {
        return Add(ContractDescriptor.Create<TContract, TImplementation>());
    }
    public ObjectTemplateBuilder Add<TContract>() where TContract : class
    {
        return Add(ContractDescriptor.Create<TContract>());
    }
    public ObjectTemplateBuilder Add<TContract>(TContract implementation) where TContract : class
    {
        return Add(ContractDescriptor.Create<TContract>(implementation));
    }
    public ObjectTemplateBuilder Add(ContractDescriptor contract)
    {
        _contracts.Add(contract);

        return this;
    }
}

public class ObjectBuilder
{
    private IObjectFactory _objectFactory;
    private readonly Dictionary<Type, ContractDescriptor> _contracts = new();

    public ObjectBuilder(IObjectFactory objectFactory, ObjectTemplate template)
        : this(objectFactory) => Add(template);
    public ObjectBuilder(IObjectFactory objectFactory)
    {
        _objectFactory = objectFactory;
    }

    public IObject Build()
    {
        return _objectFactory.Create(_contracts);
    }

    public ObjectBuilder SetObjectFactory(IObjectFactory objectFactory)
    {
        _objectFactory = objectFactory;
        return this;
    }

    public ObjectBuilder Add(ObjectTemplate template)
    {
        foreach (var contract in template.Contracts)
            _contracts.Add(contract.Type, contract);

        return this;
    }
    public ObjectBuilder OverrideWith(ObjectTemplate template)
    {
        foreach (var contract in template.Contracts)
            _contracts[contract.Type] = contract;

        return this;
    }

    public ObjectBuilder Set<TContract, TImplementation>() where TContract : class
    {
        return Set(ContractDescriptor.Create<TContract, TImplementation>());
    }
    public ObjectBuilder Set<TContract>(TContract implementation) where TContract : class
    {
        return Set(ContractDescriptor.Create<TContract>(implementation));
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
    IObject Create(Dictionary<Type, ContractDescriptor> contracts);
}
public class DefaultObjectFactory<TId> : IObjectFactory
{
    private readonly IGenerator<TId> _idGenerator;
    private readonly IContractResolver _contractResolver;

    public DefaultObjectFactory(IGenerator<TId> idGenerator, IContractResolver contractResolver)
    {
        _idGenerator = idGenerator;
        _contractResolver = contractResolver;
    }

    public IObject Create(Dictionary<Type, ContractDescriptor> contracts)
    {
        var id = _idGenerator.Generate();
        var implementations = new Dictionary<Type, object>(contracts.Count);

        foreach (var contract in contracts.Values)
            implementations[contract.Type] = _contractResolver.Resolve(contract, id);

        return new Object<TId>(id, implementations);
    }
}

public class ObjectContractResolver : IContractResolver
{
    private readonly IContractResolverChain _resolverChain;

    public ObjectContractResolver(IContractResolverChain resolverChain)
    {
        _resolverChain = resolverChain;
    }

    public object Resolve<TId>(ContractDescriptor contract, TId id)
    {
        var context = new ContractResolverContext(contract);
        context.Id = id;
        
        if (_resolverChain.Resolve(context) && context.Result is not null)
            return context.Result;

        throw new InvalidOperationException($"Can't resolve {context.Contract.Type}");
    }
}
public interface IContractResolver
{
    object Resolve<TId>(ContractDescriptor contract, TId id);
}

public class ContractResolverFromDescriptor : IContractResolverChain
{
    private readonly IContractResolverChain _implementationTypeResolver;
    public ContractResolverFromDescriptor(IContractResolverChain implementationTypeResolver)
    {
        _implementationTypeResolver = implementationTypeResolver;
    }

    public bool Resolve(ContractResolverContext context)
    {
        context.Result = context.Contract.Implementation;
        if (context.Result is not null)
            return true;

        if (context.Contract.ImplementationType is null)
            return false;

        var implementationContract = new ContractDescriptor(context.Contract.ImplementationType);
        var implementationContext = new ContractResolverContext(implementationContract);
        implementationContext.Id = context.Id;

        if (_implementationTypeResolver.Resolve(implementationContext) is false || implementationContext.Result is null)
            return false;

        context.Result = implementationContext.Result;
        return true;
    }
}
public class ContractResolverFromConstructor : IContractResolverChain
{
    private readonly IContractResolverChain _parameterResolver;
    public ContractResolverFromConstructor(IContractResolverChain parameterResolver)
    {
        _parameterResolver = parameterResolver;
    }

    public bool Resolve(ContractResolverContext context)
    {
        var constructors = context.Contract.Type.GetConstructors();

        return constructors.Any(constructor => ResolveConstructor(context, constructor));
    }

    private bool ResolveConstructor(ContractResolverContext context, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            var parameterContract = new ContractDescriptor(parameter.ParameterType);
            var parameterContext = new ContractResolverContext(parameterContract);
            parameterContext.Id = context.Id;
            parameterContext.Attributes = parameter.CustomAttributes.ToArray();

            if (_parameterResolver.Resolve(parameterContext) is false || parameterContext.Result is null)
                return false;

            resultParameters[i] = parameterContext.Result;
        }

        context.Result = constructor.Invoke(resultParameters);
        return true;
    }
}
public class ValueResolverFromStorage<TId> : IContractResolverChain
{
    private readonly IContractResolverChain _storageResolver;
    public ValueResolverFromStorage(IContractResolverChain storageResolver)
    {
        _storageResolver = storageResolver;
    }

    public bool Resolve(ContractResolverContext context)
    {
        if (context.Contract.Type.IsAssignableTo(typeof(IValue)) is false || context.Id is not TId)
            return false;

        var genericValueType = context.Contract.Type.GenericTypeArguments[0];
        var idType = typeof(TId);
        var externalValueType = typeof(ExternalValue<,>).MakeGenericType(idType, genericValueType);
        
        var storageContract = new ContractDescriptor(typeof(IStorage<TId>));
        var storageResolverContext = new ContractResolverContext(storageContract);
        storageResolverContext.Id = context.Id;

        if (context.Attributes is not null)
        {
            var locationKey = context.Attributes.Get<ValueLocationAttribute>();
            if (locationKey is not null)
                storageResolverContext.Key = locationKey.AttributeType;
        }

        if (_storageResolver.Resolve(storageResolverContext) is false || storageResolverContext.Result is null)
            return false;

        var storage = (storageResolverContext.Result as IStorage<TId>)!;
        var bucket = storage.GetBucket(genericValueType);

        context.Result = Activator.CreateInstance(externalValueType, context.Id, bucket);
        return true;
    }
}
public class ContractResolverFromContainer : IContractResolverChain
{
    private readonly IServiceProvider _services;
    public ContractResolverFromContainer(IServiceProvider services)
    {
        _services = services;
    }

    public bool Resolve(ContractResolverContext context)
    {
        context.Result = context.Key is not null
            ? _services.GetKeyedService(context.Contract.Type, context.Key)
            : _services.GetService(context.Contract.Type);

        return context.Result is not null;
    }
}
public class CompositeContractResolverChain : IContractResolverChain
{
    private readonly List<IContractResolverChain> _resolvers;
    public CompositeContractResolverChain(params IContractResolverChain[] resolvers)
    {
        _resolvers = new List<IContractResolverChain>(resolvers);
    }

    public CompositeContractResolverChain Add(IContractResolverChain resolver)
    {
        _resolvers.Add(resolver);
        return this;
    }

    public bool Resolve(ContractResolverContext context)
    {
        return _resolvers.Any(resolver => resolver.Resolve(context));
    }
}
public interface IContractResolverChain
{
    bool Resolve(ContractResolverContext context);
}
public class ContractResolverContext
{
    public ContractDescriptor Contract;
    public object? Id;
    public object? Key;
    public CustomAttributeData[]? Attributes;
    public object? Result;

    public ContractResolverContext(ContractDescriptor contract)
    {
        Contract = contract;
    }
}

public class ContractResolver : IContractResolverChain
{
    private ServiceProvider _serviceProvider;
    private object? _resolveKey;

    public ContractResolver(ServiceProvider serviceProvider) : this(serviceProvider, null)
    {
    }

    public ContractResolver(ServiceProvider serviceProvider, object? resolveKey)
    {
        _serviceProvider = serviceProvider;
        _resolveKey = resolveKey;
    }

    public bool Resolve(ContractResolverContext context)
    {
        object? result = null;
        if (_resolveKey != null)
            result = _serviceProvider.GetRequiredKeyedService(context.Contract.ImplementationType!, _resolveKey);

        result = _serviceProvider.GetRequiredService(context.Contract.ImplementationType!);
        return true;
    }
}

public class ContractResolveBuilder
{
    private readonly ServiceCollection _serviceCollection = new();
    
    private object? _resolveKey = null;


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

    public IContractResolverChain Build() 
    {
        var provider = _serviceCollection.BuildServiceProvider();
        var resolveKey = _resolveKey;
        //return new ContractResolverChain(provider, resolveKey);
        throw new NotImplementedException();
    }

    public ContractResolveBuilder SetResolveKey(object? resolveKey)
    {
        _resolveKey = resolveKey;
        return this;
    }

    public ContractResolveBuilder Add<TInterface, TImplementation>(object? key = null)
    {
        if (key == null)
            _serviceCollection.AddTransient(typeof(TInterface), ResolveFromProvider<TImplementation>);
        else
            _serviceCollection.AddKeyedTransient(typeof(TInterface), key, ResolveFromProvider<TImplementation>);
        
        return this;
    }

    public ContractResolveBuilder AddResolution<TInterface, TImplementation>(object? key = null)
    {
        if (key == null)
            _serviceCollection.AddTransient(typeof(TInterface), typeof(TImplementation));
        else
            _serviceCollection.AddKeyedTransient(typeof(TInterface), key, typeof(TImplementation));

        return this;
    }

    public ContractResolveBuilder ResolveFromAnotherKey<TType>(object? key, object? targetKey)
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
        
                    // var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
                    // var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TImplementation), genericValueType,);

                    // var instance = Activator.CreateInstance(externalValueType,);
                    // constructor.Invoke(instance, resultParameters);
                    // return instance!;
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
                    //var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TImplementation), genericValueType,);
        
                    //var instance = Activator.CreateInstance(externalValueType,);
                    //constructor.Invoke(instance, resultParameters);
                    //return instance!;
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

public class ValueLocationAttribute : Attribute { }