using System;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.Values;

using Newtonsoft.Json.Linq;

using GMutagen.v8.Generators;
using GMutagen.v8.Contracts.Resolving;
using GMutagen.v8.Contracts.Resolving.Attributes;
using GMutagen.v8.Contracts.Resolving.Nodes;
using GMutagen.v8.Objects;
using GMutagen.v8.Objects.Templates;
using GMutagen.v8.Contracts;
using GMutagen.v8.IO.Sources.Dictionary;

namespace Roguelike;

public interface ITestContract
{
    IValue<int> Number1 { get; set; }
    IValue<int> Number2 { get; set; }
}
public interface INameContract
{
    IValue<string> Name { get; set; }
}
public class DefaultNameContract : INameContract
{
    public IValue<string> Name { get; set; }
    public DefaultNameContract(IValue<string> name) => Name = name;
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

static class Program
{
    private static IServiceCollection AddStorages<TObjectId, TContractId, TSlotId, TValueId>(this IServiceCollection services)
        where TObjectId : notnull
        where TContractId : notnull
        where TSlotId : notnull
        where TValueId : notnull
    {
        var dictionaryStorageFactory = new DictionaryReadWriteFactory();
        var fileStorageFactory = new FileReadWriteFactory(dictionaryStorageFactory, "Default.txt");

        // Values ValueId:Value
        var valuesStorage = fileStorageFactory.Create<TValueId, object>("Values.txt");
        services.AddSingleton(sp => valuesStorage);
        // ContractSlots ContractId:ContractValue(SlotId:ValueId)
        var contractsStorage = fileStorageFactory.Create<TContractId, ContractValue<TSlotId, TValueId>>("Contracts.txt");
        services.AddSingleton(sp => contractsStorage);
        // Objects ObjectId:ObjectValue(ContractType:ContractId)
        var objectStorage = fileStorageFactory.Create<TObjectId, ObjectValue<TContractId>>("Objects.txt");
        services.AddSingleton(sp => objectStorage);

        return services;
    }

    public static void Main(string[] args)
    {
        var gameConfig = new ServiceCollection()
            .AddStorages<int, int, int, int>();

        using var gameServices = gameConfig.BuildServiceProvider();

        var compositeResolver = new CompositeContractResolverNode();
        compositeResolver.Add(new ContractResolverFromDescriptor(compositeResolver))
            .Add(new ContractResolverFromContainer(gameServices))
            .Add(new ValueResolverFromStorage<int, int>(compositeResolver, new IncrementalGenerator<int>()))
            .Add(new ContractResolverFromConstructor<int, int, int>(gameServices, compositeResolver, new IncrementalGenerator<int>()));

        var objectFactory = new ResolvingObjectFactory<int, int>(gameServices, new IncrementalGenerator<int>(), compositeResolver);


        var snakeTemplate = new ObjectTemplateBuilder()
            //.Add<INameContract, DefaultNameContract>()
            .Add<ITestContract, TestContract>()
            .Build();

        var snakeBuilder = new ObjectBuilder<int>(objectFactory, snakeTemplate);

        Console.WriteLine("Snakes with god snake:");

        var godSnake = snakeBuilder.Build();
        var godContract = godSnake.Get<ITestContract>();

        Console.WriteLine("GodObject Id - " + godSnake.Id);
        Console.WriteLine("GodBefore");
        Console.WriteLine(godContract.Number1.Value);
        Console.WriteLine(godContract.Number2.Value);
        // Console.WriteLine(godContract.NameContract.Name);


        godContract.Number1.Value += 1;
        godContract.Number2.Value += 2;
        // godContract.NameContract.Name.Value = "Amoga - " + (godContract.Number1.Value + godContract.Number2.Value);

        Console.WriteLine("GodAfter");
        Console.WriteLine(godContract.Number1.Value);
        Console.WriteLine(godContract.Number2.Value);
        Console.WriteLine();

        snakeBuilder.Set(godContract);
        for (var i = 0; i < 3; i++)
        {
            var snake = snakeBuilder.Build();
            var testContract = snake.Get<ITestContract>();

            Console.WriteLine("Object Id - " + snake.Id);
            Console.WriteLine("Before");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);

            godContract.Number1.Value += 1;
            godContract.Number2.Value += 2;

            Console.WriteLine("After");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);
            Console.WriteLine();
        }

        Console.WriteLine("GodAfter this snakes");
        Console.WriteLine(godContract.Number1.Value);
        Console.WriteLine(godContract.Number2.Value);
        Console.WriteLine();

        snakeBuilder.Set<ITestContract, TestContract>();

        Console.WriteLine("Simple snakes:");
        for (var i = 0; i < 3; i++)
        {
            var snake = snakeBuilder.Build();
            var testContract = snake.Get<ITestContract>();

            Console.WriteLine("Object Id - " + snake.Id);
            Console.WriteLine("Before");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);

            testContract.Number1.Value += i;
            testContract.Number2.Value += i * 2;

            Console.WriteLine("After");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);
            Console.WriteLine();
        }

        gameServices.Dispose();

        Console.ReadKey(true);
    }
}