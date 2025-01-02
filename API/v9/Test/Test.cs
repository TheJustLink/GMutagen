/*using GMutagen.v9.Contracts;
using GMutagen.v9.Contracts.Resolving.Contexts;
using GMutagen.v9.Generators;
using GMutagen.v9.Contracts.Resolving.Nodes;
using GMutagen.v9.Contracts.Resolving.Nodes.From.Cache;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData;
using GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Creation;
using GMutagen.v9.Objects;
using GMutagen.v9.Values;
using Microsoft.Extensions.DependencyInjection;
using DictionaryReadWriteFactory = GMutagen.v9.IO.Sources.Dictionary.DictionaryReadWriteFactory;

namespace GMutagen.v9.Test;


public class Test
{
    public void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        
        var dictionaryReadWriteFactory = new DictionaryReadWriteFactory();
        
        var fromCollectionCache = new FromCollectionCache(serviceCollection);
        
        
       
        
        var objectIdGenerator = new IncrementalGenerator<int>();
        var objectMetaStorage = dictionaryReadWriteFactory
            .CreateReadWrite<int, ObjectMetaData<int>>();
        
        var createObjectMeta = new CreateObjectMetaData<int, int>(objectIdGenerator, objectMetaStorage);

        var objectResolver = new CompositeResolverNode()
            .Add(createObjectMeta);
        
        var fromObjectMetaCache = new FromObjectMetaCache();
        
        var fullObjectResolver = new CompositeResolverNode()
            .Add(fromObjectMetaCache)
            .Add(fromCollectionCache)
            .Add(objectResolver);


        var resolveObjectNeta = new FromAllContextsCache();
        var contractIdGenerator = new IncrementalGenerator<int>();
        var contractMetaStorage = dictionaryReadWriteFactory
            .CreateReadWrite<int, ContractMetaData<int, int>>();

        
        var contractMeta = new CreateContractMetaData<int, int>(resolveObjectNeta, contractIdGenerator, contractMetaStorage);
        
        
        var fullContractResolver = new CompositeResolverNode()
            .Add()
        
       

      
        
    
        var valueMetaStorage = dictionaryReadWriteFactory
            .CreateReadWrite<int, ValueMetaData<int>>();
        
        
        var valueIdGenerator = new IncrementalGenerator<int>();

     
     
        var valueMeta = new CreateValueMetaData<int>(, valueIdGenerator,valueMetaStorage);


        
       
        var fromContractMetaCache = new FromMetaCache();
        var fromValueMetaCache = new FromValueMetaCache();

        var resolver = new CompositeResolverNode()
            .Add(fromObjectMetaCache)
            .Add(fromContractMetaCache)
            .Add(fromValueMetaCache)
            .Add(fromCollectionCache)
            .Add();
    }
}

public class FromValueMetaCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!typeof(IValue).IsAssignableFrom(context.Type))
            return false;



        return true;
    }
}

public class FromMetaCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!typeof(IContract).IsAssignableFrom(context.Type))
            return false;


        return true;
    }
}

public class FromObjectMetaCache : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!typeof(IObject).IsAssignableFrom(context.Type))
            return false;

        return true;
    }
}*/