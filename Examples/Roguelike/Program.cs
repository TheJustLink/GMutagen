using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.Values;
using GMutagen.Objects;
using GMutagen.Contracts;
using GMutagen.Contracts.Resolving;
using GMutagen.Contracts.Resolving.Nodes;
using GMutagen.Generators;
using GMutagen.IO;
using GMutagen.IO.Sources.Dictionary;
using GMutagen.Objects.Templates;

namespace Roguelike;

public interface ITestContract
{
    IValue<int> Number1 { get; set; }
    IValue<int> Number2 { get; set; }

    void Test();
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

    private readonly IValue<string> _name;

    public TestContract(IValue<int> number1, IValue<int> number2, INameContract nameContract)
    {
        Number1 = number1;
        Number2 = number2;

        _name = nameContract.Name;
    }

    public void Test()
    {
        Console.WriteLine($"[{_name.Value}] {Number1.Value} + {Number2.Value} = {Number1.Value + Number2.Value}");
    }
}
public class TestContract2 : ITestContract
{
    public IValue<int> Number1 { get; set; }
    public IValue<int> Number2 { get; set; }
    
    private readonly IValue<string> _state;
    private readonly IValue<string> _name;

    public TestContract2(IValue<int> number1, IValue<int> number2, IValue<string> state, INameContract nameContract)
    {
        Number1 = number1;
        Number2 = number2;
        _state = state;
        _name = nameContract.Name;
    }

    public void Test()
    {
        Console.WriteLine($"[{_name.Value}] State = {_state.Value}");

        if (_state.Value == "StartCalculation")
        {
            Console.WriteLine($"{Number1.Value} + {Number2.Value} = {Number1.Value + Number2.Value}");
            _state.Value = "Calculated";
        }
        else if (string.IsNullOrEmpty(_state.Value))
        {
            _state.Value = "StartCalculation";
        }
        else if (_state.Value == "Calculated")
        {
            Console.WriteLine("Already calculated!");
        }
    }
}

static class Program
{
    private static IServiceCollection AddStorages<TObjectId, TContractId, TSlotId, TValueId>(this IServiceCollection services,
        out int objectsCount, out int contractsCount, out int valuesCount)
        where TObjectId : notnull
        where TContractId : notnull
        where TSlotId : notnull
        where TValueId : notnull
    {
        var dictionaryStorageFactory = new DictionaryReadWriteFactory();
        var fileStorageFactory = new FileReadWriteFactory(dictionaryStorageFactory, "Default.txt");

        // Values ValueId:Value
        var valuesStorage = fileStorageFactory.Create<TValueId, object>("Values.txt");
        valuesCount = valuesStorage.Count;
        services.AddSingleton(sp => valuesStorage);
        // ContractSlots ContractId:ContractValue(SlotId:ValueId)
        var contractsStorage = fileStorageFactory.Create<TContractId, ContractValue<TSlotId, TValueId>>("Contracts.txt");
        contractsCount = contractsStorage.Count;
        services.AddSingleton(sp => contractsStorage);
        // Objects ObjectId:ObjectValue(ContractType:ContractId)
        var objectStorage = fileStorageFactory.Create<TObjectId, ObjectValue<TContractId>>("Objects.txt");
        objectsCount = objectStorage.Count;
        services.AddSingleton(sp => objectStorage);

        return services;
    }

    public static void Main(string[] args)
    {
        var gameConfig = new ServiceCollection()
            .AddStorages<int, int, int, int>(out var objectsCount, out var contractsCount, out var valuesCount);

        using var gameServices = gameConfig.BuildServiceProvider();

        var valueIdGenerator = new IncrementalGenerator<int>(valuesCount);
        var contractIdGenerator = new IncrementalGenerator<int>(contractsCount);
        var objectIdGenerator = new IncrementalGenerator<int>(objectsCount);

        var compositeResolver = new CompositeContractResolverNode();
        compositeResolver.Add(new ContractResolverFromDescriptor<int>(compositeResolver, contractIdGenerator))
            .Add(new ContractResolverFromContainer(gameServices))
            .Add(new ValueResolverFromStorage<int, int>(compositeResolver, valueIdGenerator))
            .Add(new ContractResolverFromConstructor<int, int, int>(gameServices, compositeResolver, contractIdGenerator));

        var objectFactory = new ResolvingObjectFactory<int, int, int, int>(gameServices, objectIdGenerator, compositeResolver);

        var snakeTemplate = new ObjectTemplateBuilder()
            //.Add<INameContract, DefaultNameContract>()
            .Add<INameContract, DefaultNameContract>()
            .Add<ITestContract, TestContract>()
            .Build();

        var snakeBuilder = new ObjectBuilder<int>(objectFactory, snakeTemplate);

        if (objectsCount != 0)
        {
            for (var i = 0; i < objectsCount; i++)
            {
                var snake = snakeBuilder.Build(i);
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
                Console.WriteLine("Test:");
                testContract.Test();
                Console.WriteLine();
            }

            return;
        }

        Console.WriteLine("Snakes with god snake:");

        var godSnake = snakeBuilder.Build();
        var godContract = godSnake.Get<ITestContract>();
        var godNameContract = godSnake.Get<INameContract>();

        if (string.IsNullOrEmpty(godNameContract.Name.Value))
            godNameContract.Name.Value = "GOD SNAKE";

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
        Console.WriteLine("Test:");
        godContract.Test();
        Console.WriteLine();

        snakeBuilder.Set(godContract);
        snakeBuilder.Set(godNameContract);
        for (var i = 0; i < 3; i++)
        {
            IObject<int> snake;

            if (i == 0)
            {
                snakeBuilder.Set<ITestContract, TestContract2>();
                snakeBuilder.Set<INameContract, DefaultNameContract>();
                
                snake = snakeBuilder.Build();
                var nameContract = snake.Get<INameContract>();
                if (string.IsNullOrEmpty(nameContract.Name.Value))
                    nameContract.Name.Value = "Strange snake";

                snakeBuilder.Set(godContract);
                snakeBuilder.Set(godNameContract);
            }
            else
            {
                snake = snakeBuilder.Build();
            }

            var testContract = snake.Get<ITestContract>();

            Console.WriteLine("Object Id - " + snake.Id);
            Console.WriteLine("Before");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);

            testContract.Number1.Value += 1;
            testContract.Number2.Value += 2;

            Console.WriteLine("After");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);
            Console.WriteLine("Test:");
            testContract.Test();
            Console.WriteLine();
        }

        Console.WriteLine("GodAfter this snakes");
        Console.WriteLine(godContract.Number1.Value);
        Console.WriteLine(godContract.Number2.Value);
        Console.WriteLine();

        snakeBuilder.Set<ITestContract, TestContract>();
        snakeBuilder.Set<INameContract, DefaultNameContract>();

        INameContract? simpleSnakeContract = default;

        Console.WriteLine("Simple snakes:");
        for (var i = 0; i < 3; i++)
        {
            var snake = snakeBuilder.Build();
            var testContract = snake.Get<ITestContract>();
            
            if (simpleSnakeContract is null)
            {
                var nameContract = snake.Get<INameContract>();
                if (string.IsNullOrEmpty(nameContract.Name.Value))
                    nameContract.Name.Value = "Simple snake";

                simpleSnakeContract = nameContract;
                snakeBuilder.Set(simpleSnakeContract);
            }

            Console.WriteLine("Object Id - " + snake.Id);
            Console.WriteLine("Before");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);

            testContract.Number1.Value += i;
            testContract.Number2.Value += i * 2;

            Console.WriteLine("After");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);
            Console.WriteLine("Test:");
            testContract.Test();
            Console.WriteLine();
        }

        gameServices.Dispose();

        Console.ReadKey(true);
    }
}

public class Migration<TValueId, TContractId, TSlotId, TObjectId>
{
    private IReadWrite<TValueId, object> _objects;
    private IReadWrite<TContractId, ContractValue<TSlotId, TValueId>> _contracts;
    private IReadWrite<TObjectId, ObjectValue<TContractId>> _values;

    private List<MigrationOperation> _operations;

    public Migration(IReadWrite<TValueId, object> objects, IReadWrite<TContractId, ContractValue<TSlotId, TValueId>> contracts, IReadWrite<TObjectId, ObjectValue<TContractId>> values) 
    {
        _objects = objects;
        _contracts = contracts;
        _values = values;
        _operations = new List<MigrationOperation>();
    }

    public Migration<TValueId, TContractId, TSlotId, TObjectId> ChangeRealization()
    {
        _operations.Add(new ChangeRealization());
        return this;
    }

    public Migration<TValueId, TContractId, TSlotId, TObjectId> ChangeContract()
    {
        _operations.Add(new ChangeContract());
        return this;
    }

    public Migration<TValueId, TContractId, TSlotId, TObjectId> ChangeContractAndRealization() 
    {
        _operations.Add(new ChangeContractAndRealization());
        return this; 
    }
}

public class MigrationOperation
{
}

public class ChangeContractAndRealization : MigrationOperation
{ 
}
public class ChangeContract  : MigrationOperation 
{ 
}
public class ChangeRealization : MigrationOperation
{ 
}
