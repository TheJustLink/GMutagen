using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.IO;
using GMutagen.v8.IO.Repositories;
using GMutagen.v8.Objects;
using GMutagen.v8.Objects.Template;
using GMutagen.v8.Values;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        return attributes.Any(attribute => attribute.AttributeType.IsAssignableTo(typeof(T)));
    }
    public static CustomAttributeData? Get<T>(this IEnumerable<CustomAttributeData> attributes)
    {
        return attributes.FirstOrDefault(attribute => attribute.AttributeType.IsAssignableTo(typeof(T)));
    }
}

public class InMemoryAttribute : ValueLocationAttribute { }

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectsInMemory<TId, TSlotId>(this IServiceCollection services, Func<IReadWrite<Type, TSlotId>> factory)
    {
        return services.AddContractSlotsInMemory<TId, Type, TSlotId>(factory);
    }

    public static IServiceCollection AddContractSlotsInMemory<TId, TSlotId, TValue>(this IServiceCollection services, Func<IReadWrite<TSlotId, TValue>> factory)
    {
        return AddDeepStorage<TId, TSlotId, TValue>(services, factory);
    }
    public static IServiceCollection AddDeepStorage<TId, TDeepId, TValue>(this IServiceCollection services, Func<IReadWrite<TDeepId, TValue>> factory)
    {
        var dictionary = new Dictionary<TId, IReadWrite<TDeepId, TValue>>();
        
        IRead<TId, IReadWrite<TDeepId, TValue>> contractSlotsReader = new DictionaryRead<TId, IReadWrite<TDeepId, TValue>>(dictionary);
        IWrite<TId, IReadWrite<TDeepId, TValue>> contractSlotsWriter = new DictionaryWrite<TId, IReadWrite<TDeepId, TValue>>(dictionary);
        
        contractSlotsReader = new LazyRead<TId, IReadWrite<TDeepId, TValue>>(contractSlotsReader, contractSlotsWriter, factory);
        var contractSlotsReadWrite = new ReadWrite<TId, IReadWrite<TDeepId, TValue>>(contractSlotsReader, contractSlotsWriter);

        return services.AddSingleton<IReadWrite<TId, IReadWrite<TDeepId, TValue>>>(contractSlotsReadWrite);
    }

    public static IServiceCollection AddStorage<TId, TValue>(this IServiceCollection services, Func<IReadWrite<TId, TValue>> factory) 
    {
        return services.AddSingleton<IReadWrite<TId, TValue>>(factory());
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
public class DefaultObjectFactory<TId> : IObjectFactory where TId : notnull
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
        Console.WriteLine("CREATING OBJECT " + id);
        var implementations = new Dictionary<Type, object>(contracts.Count);

        foreach (var contract in contracts.Values)
            implementations[contract.Type] = _contractResolver.Resolve(contract, id);

        return new Object<TId>(id, implementations);
    }
}

public class ObjectContractResolver : IContractResolver
{
    private readonly IContractResolverChain _resolverChain;
    private readonly IServiceCollection _buildServices;

    public ObjectContractResolver(IContractResolverChain resolverChain, IServiceCollection buildServices)
    {
        _resolverChain = resolverChain;
        _buildServices = buildServices;
    }

    public object Resolve<TId>(ContractDescriptor contract, TId id) where TId : notnull
    {
        var buildServices = new ServiceCollection { _buildServices };
        buildServices.AddSingleton(new ObjectId(typeof(TId), id));

        var context = new ContractResolverContext(contract, buildServices);
        
        if (_resolverChain.Resolve(context) && context.Result is not null)
            return context.Result;

        throw new InvalidOperationException($"Can't resolve {context.Contract.Type}");
    }
}
class ObjectId : Id
{
    public ObjectId(Type type, object value) : base(type, value) { }
}
class ContractId : Id
{
    public ContractId(Type type, object value) : base(type, value) { }
}
class SlotId : Id
{
    public SlotId(Type type, object value) : base(type, value) { }
}
class ValueId : Id
{
    public ValueId(Type type, object value) : base(type, value) { }
}
public class Id
{
    public readonly Type Type;
    public readonly object Value;

    public Id(Type type, object value)
    {
        Type = type;
        Value = value;
    }
}
public interface IContractResolver
{
    object Resolve<TId>(ContractDescriptor contract, TId id) where TId : notnull;
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
        var implementationContext = new ContractResolverContext(implementationContract, context.BuildServices);

        if (_implementationTypeResolver.Resolve(implementationContext) is false || implementationContext.Result is null)
            return false;

        context.Result = implementationContext.Result;
        return true;
    }
}
public class ContractResolverFromConstructor<TObjectId, TContractId> : IContractResolverChain
{
    private readonly IContractResolverChain _parameterResolver;
    private readonly IGenerator<TContractId> _contractIdGenerator;
    public ContractResolverFromConstructor(IContractResolverChain parameterResolver, IGenerator<TContractId> contractIdGenerator)
    {
        _parameterResolver = parameterResolver;
        _contractIdGenerator = contractIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        var services = context.Services;
        var objectIdDescriptor = services.GetRequiredService<ObjectId>();

        var objects = services.GetRequiredService<IReadWrite<TObjectId, IReadWrite<Type, TContractId>>>();
        if (objectIdDescriptor.Value is not TObjectId objectId)
            return false;

        var contractType = context.Contract.Type;
        var contracts = objects[objectId];
        TContractId contractId;
        
        if (contracts.Contains(contractType))
        {
            contractId = contracts[contractType];
        }
        else
        {
            contractId = _contractIdGenerator.Generate();
            contracts[contractType] = contractId;
        }
        context.BuildServices.AddSingleton(new ContractId(typeof(TContractId), contractId));

        var constructors = contractType.GetConstructors();
        return constructors.Any(constructor => ResolveConstructor(context, constructor));
    }

    private bool ResolveConstructor(ContractResolverContext context, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var resultParameters = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];
            context.BuildServices.AddSingleton(new SlotId(i.GetType(), i));

            if (ResolveParameter(context, parameterInfo, out var parameter) is false)
                return false;

            resultParameters[i] = parameter!;
        }

        context.Result = constructor.Invoke(resultParameters);
        return true;
    }
    private bool ResolveParameter(ContractResolverContext context, ParameterInfo parameterInfo, out object? parameter)
    {
        parameter = default;

        var parameterContract = new ContractDescriptor(parameterInfo.ParameterType);
        var parameterContext = new ContractResolverContext(parameterContract, context.BuildServices);
        parameterContext.Attributes = parameterInfo.CustomAttributes.ToArray();

        if (_parameterResolver.Resolve(parameterContext) is false || parameterContext.Result is null)
            return false;

        parameter = parameterContext.Result;

        return true;
    }
}
public class ValueResolverFromStorage<TContractId, TSlotId, TValueId> : IContractResolverChain
{
    private readonly IContractResolverChain _storageResolver;
    private readonly IGenerator<TValueId> _valueIdGenerator;
    public ValueResolverFromStorage(IContractResolverChain storageResolver, IGenerator<TValueId> valueIdGenerator)
    {
        _storageResolver = storageResolver;
        _valueIdGenerator = valueIdGenerator;
    }

    public bool Resolve(ContractResolverContext context)
    {
        if (context.Contract.Type.IsAssignableTo(typeof(IValue)) is false)
            return false;

        return ResolveStorage(context, out var storage) && Resolve(context, storage!);
    }

    private bool Resolve(ContractResolverContext context, IReadWrite<TValueId, object> valuesStorage)
    {
        var services = context.Services;
        var contractIdDescriptor = services.GetRequiredService<ContractId>();
        var slotIdDescriptor = services.GetRequiredService<SlotId>();

        var contracts = services.GetRequiredService<IReadWrite<TContractId, IReadWrite<TSlotId, TValueId>>>();
        if (contractIdDescriptor.Value is not TContractId contractId || slotIdDescriptor.Value is not TSlotId slotId)
            return false;

        var slots = contracts.Read(contractId);
        TValueId valueId;
        if (slots.Contains(slotId))
        {
            valueId = slots[slotId];
        }
        else
        {
            valueId = _valueIdGenerator.Generate();
            slots[slotId] = valueId;
        }

        var valueType = context.Contract.Type.GenericTypeArguments[0];
        context.Result = CreateExternalValue(valueType, valueId, valuesStorage);

        return true;
    }
    private bool ResolveStorage(ContractResolverContext context, out IReadWrite<TValueId, object>? storage)
    {
        storage = null;

        var valuesStorageType = typeof(IReadWrite<TValueId, object>);
        var storageContract = new ContractDescriptor(valuesStorageType);
        var storageResolverContext = new ContractResolverContext(storageContract, context.BuildServices);

        if (context.Attributes is not null)
        {
            var locationKey = context.Attributes.Get<ValueLocationAttribute>();
            if (locationKey is not null)
                storageResolverContext.Key = locationKey.AttributeType;
        }

        if (_storageResolver.Resolve(storageResolverContext) is false || storageResolverContext.Result is null)
            return false;

        storage = (storageResolverContext.Result as IReadWrite<TValueId, object>)!;
        return true;
    }
    private object CreateExternalValue(Type valueType, TValueId valueId, IReadWrite<TValueId, object> storage)
    {
        var idType = typeof(TValueId);
        var readWriteTypeCastedType = typeof(ReadWriteTypeCasted<,>).MakeGenericType(idType, valueType);
        var externalValueType = typeof(ExternalValue<,>).MakeGenericType(idType, valueType);

        var readWriteTypeCasted = Activator.CreateInstance(readWriteTypeCastedType, storage)!;
        return Activator.CreateInstance(externalValueType, valueId, readWriteTypeCasted)!;
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
    public readonly ContractDescriptor Contract;
    public readonly IServiceCollection BuildServices;

    public object? Key;
    public object? Result;

    public CustomAttributeData[]? Attributes;

    public ContractResolverContext(ContractDescriptor contract, IServiceCollection services)
    {
        Contract = contract;
        BuildServices = services;
    }

    public IServiceProvider Services => BuildServices.BuildServiceProvider();
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
                    // var storage = locationKey is not null
                    //     ? serviceProvider.GetRequiredKeyedService<IStorage<TImplementation>>(locationKey)
                    //     : serviceProvider.GetRequiredService<IStorage<TImplementation>>();
                    //
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
                    // var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
                    // var storage = locationKey is not null
                    // ? serviceProvider.GetRequiredKeyedService<IStorage<TImplementation>>(locationKey)
                    //     : serviceProvider.GetRequiredService<IStorage<TImplementation>>();
                    //
                    // var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
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


public class TestMemory<TValue> : IReadWrite<int, TValue>, IGenerator<int>
{
    private readonly List<TValue> _memory = new();
    private readonly LinkedList<int> _nextFree = new();

    public TValue this[int id]
    {
        get => Read(id);
        set => Write(id, value);
    }

    public TValue Read(int id)
    {
        return _memory[id];
    }

    public void Write(int id, TValue value)
    {
        if (id < _memory.Count)
            _memory[id] = value;
        else
            _memory.Add(value);
    }

    public void Release(int id)
    {
        _nextFree.AddFirst(id);
    }

    public int Generate()
    {
        if (_nextFree.First == null)
            return _memory.Count;

        var value = _nextFree.First.Value;
        _nextFree.RemoveFirst();
        return value;
    }

    public bool Contains(int id)
    {
        return id < _memory.Count;
    }
}