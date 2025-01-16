using GMutagen.v9.Contracts;
using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.Contracts.Resolving;
using GMutagen.v9.Contracts.Resolving.Contexts.Key;
using GMutagen.v9.Contracts.Resolving.Nodes;
using GMutagen.v9.Contracts.Resolving.Nodes.Cache;
using GMutagen.v9.Contracts.Resolving.Nodes.Cache.Collection;
using GMutagen.v9.Contracts.Resolving.Nodes.Cache.Context;
using GMutagen.v9.Contracts.Resolving.Nodes.Cache.MetaData;
using GMutagen.v9.Contracts.Resolving.Nodes.Composite;
using GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;
using GMutagen.v9.Contracts.Resolving.Nodes.From.Cache.Collection;
using GMutagen.v9.Contracts.Resolving.Nodes.From.Cache.Context;
using GMutagen.v9.Contracts.Resolving.Nodes.Internal.Common;
using GMutagen.v9.Contracts.Resolving.Nodes.Internal.Resolvers;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Creation;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Models;
using GMutagen.v9.Generators;
using GMutagen.v9.IO.Sources.Dictionary;
using GMutagen.v9.Logging;
using GMutagen.v9.Logging.Logger;
using GMutagen.v9.Logging.Logger.Common;
using GMutagen.v9.Logging.Messages;
using GMutagen.v9.Objects;
using GMutagen.v9.Objects.Factories;
using GMutagen.v9.Objects.Templates;
using GMutagen.v9.Values;
using GMutagen.v9.Values.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

public interface ITestContract : IContract
{
    IValue<int> Number1 { get; set; }
    IValue<int> Number2 { get; set; }

    void Test();
}

public interface INameContract : IContract
{
    IValue<string> Name { get; set; }
}

public class DefaultNameContract(IValue<string> name) : INameContract
{
    public IValue<string> Name { get; set; } = name;
}

public class TestContract(IValue<int> number1, IValue<int> number2, INameContract nameContract)
    : ITestContract
{
    public IValue<int> Number1 { get; set; } = number1;
    public IValue<int> Number2 { get; set; } = number2;

    private readonly IValue<string> _name = nameContract.Name;

    public void Test()
    {
        Console.WriteLine(
            $"TestContract [{_name.Value}] {Number1.Value} + {Number2.Value} = {Number1.Value + Number2.Value}");
    }
}

public class TestContract2(IValue<int> number1, IValue<int> number2, IValue<string> state, INameContract nameContract)
    : ITestContract
{
    public IValue<int> Number1 { get; set; } = number1;
    public IValue<int> Number2 { get; set; } = number2;

    public void Test()
    {
        Console.WriteLine($"TestContract2");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var templateString = $"[{{{nameof(PlaceHolders.Date)}}} " +
                             $"{{{nameof(PlaceHolders.Time)}}}] " +
                             $"[{{{nameof(PlaceHolders.Level)}}}] " +
                             $"{{{nameof(PlaceHolders.Message)}}}";
        
        var logger = new ConsoleLogger<Global>(new InterpolationString(templateString).AddInfo());

        var serviceCollection = new ServiceCollection();

        var dictionaryReadWriteFactory = new DictionaryReadWriteFactory();

        var fromCollectionCache = new FromCollectionCache(serviceCollection, logger);

        var metaFromAllContexts = new FromAllContextsCaches(logger);
        var declaredTypeFromAllContexts = new FromAllContextsCaches(logger);

        var storageResolverNode = new StorageResolver(logger);
        var cacheStorageResolverNode =
            new CacheResultInCollection(storageResolverNode, serviceCollection, [KeyType.DeclaredType], logger);

        var fullStorageResolver = new CompositeResolverNode()
            .Add(fromCollectionCache)
            .Add(cacheStorageResolverNode);

        var valueMetaStorage = dictionaryReadWriteFactory
            .CreateReadWrite<int, ValueMetaData<int>>();

        var createValueMeta = new CreateValueMetaData<int>(metaFromAllContexts, valueMetaStorage, logger);
        var valueResolverNode = new ValueResolver<int>(fullStorageResolver, logger);

        var valueResolver = new CompositeResolverNode()
            .Add(createValueMeta)
            .Add(valueResolverNode);

        var fromValueMetaCache = new ValueFromMetaCache<int, int>(fullStorageResolver, KeyType.Id, valueMetaStorage, logger);

        var fullValueResolver = new CompositeResolverNode()
            .Add(fromValueMetaCache)
            .Add(fromCollectionCache)
            .Add(valueResolver);


        var valueIdGenerator = new IncrementalGenerator<int>();
        var contractMetaStorage = dictionaryReadWriteFactory
            .CreateReadWrite<int, ContractMetaData<int, int>>();

        var createContractMeta = new CreateContractMetaData<int, int>(metaFromAllContexts, contractMetaStorage, logger);
        var fullContractResolver = new CompositeResolverNode();

        var contractResolverNode = new ContractResolver<int>(fullContractResolver, valueIdGenerator, logger);
        var cacheContractResolverNode =
            new CacheResultInRootContextByKeys(contractResolverNode, [KeyType.DeclaredType], logger);

        var contractResolver = new CompositeResolverNode()
            .Add(createContractMeta)
            .Add(cacheContractResolverNode);

        var fromContractMetaCache =
            new ContractFromMetaCache<int, int>(fullContractResolver, KeyType.Id, contractMetaStorage, logger);

        var cacheFromContractMetaCache =
            new CacheResultInRootContextByKeys(fromContractMetaCache, [KeyType.DeclaredType], logger);

        var mapContractInterface = new MapContractInterface(fullContractResolver, logger);

        fullContractResolver
            .Add(declaredTypeFromAllContexts)
            .Add(mapContractInterface)
            .Add(cacheFromContractMetaCache)
            .Add(fromCollectionCache)
            .Add(contractResolver)
            .Add(fullValueResolver);


        var contractIdGenerator = new IncrementalGenerator<int>();
        var objectMetaStorage = dictionaryReadWriteFactory
            .CreateReadWrite<int, ObjectMetaData<int>>();

        var createObjectMeta = new CreateObjectMetaData<int, int>(objectMetaStorage, logger);
        var objectResolverNode = new ObjectResolver<int, int>(fullContractResolver, contractIdGenerator, logger);

        var objectResolver = new CompositeResolverNode()
            .Add(createObjectMeta)
            .Add(objectResolverNode);

        var fromObjectMetaCache =
            new ObjectFromMetaCache<int>(fullContractResolver, KeyType.Id, objectMetaStorage, logger);

        var fullObjectResolver = new CompositeResolverNode()
            .Add(fromObjectMetaCache)
            .Add(fromCollectionCache)
            .Add(objectResolver);

        var resolver = new CompositeResolverNode()
            .Add(fullObjectResolver);


        var objectIdGenerator = new IncrementalGenerator<int>();

        var objectFactory = new ResolvingObjectFactory<int>(objectIdGenerator, resolver, logger);

        var snakeTemplate = new ObjectTemplateBuilder()
            .Add<INameContract, DefaultNameContract>()
            .Add<ITestContract, TestContract>()
            .Build();


        var snakeBuilder = new ObjectBuilder<int>(objectFactory, snakeTemplate);

        var a = typeof(INameContract).IsAssignableTo(typeof(IContract));

        var snake1 = snakeBuilder.Build();
        var snake2 = snakeBuilder.Build(0);
        Console.ReadKey();
    }
}