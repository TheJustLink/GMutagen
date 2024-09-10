using System;

using GMutagen.v8.Id;
using GMutagen.v8.IO;
using GMutagen.v8.IO.Repositories;
using GMutagen.v8.Test;
using GMutagen.v8.Values;

using Microsoft.Extensions.DependencyInjection;

namespace Roguelike;

public class InLoggerAttribute : ValueLocationAttribute { }

public interface ITestContract
{
    IValue<int> Number1 { get; set; }
    IValue<int> Number2 { get; set; }
}

public class TestContract : ITestContract
{
    public IValue<int> Number1 { get; set; }
    public IValue<int> Number2 { get; set; }

    public TestContract(IValue<int> number1, IValue<int> number2)
    {
        Number1 = number1;
        Number2 = number2;
    }
}
public class LoggerRepository<TId, TValue> : IReadWrite<TId, TValue>
{
    private readonly IReadWrite<TId, TValue> _source;

    public TValue this[TId id]
    {
        get => Read(id);
        set => Write(id, value);
    }

    public LoggerRepository(IReadWrite<TId, TValue> source)
    {
        _source = source;
    }

    public void Write(TId id, TValue value)
    {
        Console.WriteLine($"Write [{id}] <= [{value}]");
        _source.Write(id, value);
    }
    public TValue Read(TId id)
    {
        var value = _source.Read(id);
        Console.WriteLine($"Read [{id}] => [{value}]");

        return value;
    }
}
public class LoggerBucketFactory<TId> : IBucketFactory<TId>
{
    public IReadWrite<TId, T> Create<T>() => new LoggerRepository<TId, T>(new MemoryRepository<TId, T>());
    public object Create(Type valueType)
    {
        var loggerOpenType = typeof(LoggerRepository<,>);
        var loggerClosedType = loggerOpenType.MakeGenericType(typeof(TId), valueType);

        var memoryOpenType = typeof(MemoryRepository<,>);
        var memoryClosedType = memoryOpenType.MakeGenericType(typeof(TId), valueType);

        var memoryRepository = Activator.CreateInstance(memoryClosedType)!;
        var loggerRepository = Activator.CreateInstance(loggerClosedType, memoryRepository)!;

        return loggerRepository;
    }
}

static class Program
{
    public static void Main(string[] args)
    {
        var gameConfig = new ServiceCollection();
        gameConfig.AddDefaultMemoryStorage<int>();
        gameConfig.AddStorage<int, InLoggerAttribute>(new LoggerBucketFactory<int>());

        var gameServices = gameConfig.BuildServiceProvider();

        // Using

        var defaultStorage = gameServices.GetRequiredService<IStorage<int>>();
        var inMemoryStorage = gameServices.GetRequiredKeyedService<IStorage<int>>(typeof(InMemoryAttribute));

        var floatBucket = defaultStorage.GetBucket<float>();
        var externalValue = new ExternalValue<int, float>(0, floatBucket);

        externalValue.Value = 10;

        var compositeResolver = new CompositeContractResolverChain();
        compositeResolver.Add(new ContractResolverFromDescriptor(compositeResolver))
            .Add(new ContractResolverFromContainer(gameServices))
            .Add(new ValueResolverFromStorage<int>(compositeResolver))
            .Add(new ContractResolverFromConstructor(compositeResolver));


        var objectContractResolver = new ObjectContractResolver(compositeResolver);
        var objectFactory = new DefaultObjectFactory<int>(new IncrementalGenerator<int>(), objectContractResolver);




        var snakeTemplate = new ObjectTemplateBuilder()
            .Add<ITestContract, TestContract>()
            .Build();

        var snake = new ObjectBuilder(objectFactory, snakeTemplate)
            //.Set<ITestContract, TestContract>()
            .Build();

        var testContract = snake.Get<ITestContract>();

        Console.WriteLine(snake);
        Console.WriteLine(testContract);

        testContract.Number1.Value = 123;
        testContract.Number2.Value = 228;

        Console.WriteLine(testContract.Number1.Value);
        Console.WriteLine(testContract.Number2.Value);


        Console.ReadKey(true);
    }
}