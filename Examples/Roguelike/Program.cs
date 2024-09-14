using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.Id;
using GMutagen.v8.IO;
using GMutagen.v8.IO.Repositories;
using GMutagen.v8.Test;
using GMutagen.v8.Values;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;
using System.Security.Cryptography;

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
    public bool Contains(TId id)
    {
        var value = _source.Contains(id);
        Console.WriteLine($"Contains [{id}] => [{value}]");

        return value;
    }

    public static LoggerRepository<TId, TValue> Create(IReadWrite<TId, TValue> source)
    {
        return new LoggerRepository<TId, TValue>(source);
    }
}
public class FileRepository<TId, TValue> : IReadWrite<TId, TValue>
{
    private readonly IReadWrite<TId, TValue> _source;
    private readonly FileStream _stream;
    private readonly Dictionary<TId, TValue> _lines;

    public FileRepository(IReadWrite<TId, TValue> source, string filepath)
    {
        _source = source;

        if (File.Exists(filepath))
        {
            _stream = File.Open(filepath, FileMode.OpenOrCreate);
            var reader = new StreamReader(_stream);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TypeAwareConverter());

            _lines = JsonConvert.DeserializeObject<Dictionary<TId, TValue>>(reader.ReadToEnd(), settings) ?? new();

            foreach (var line in _lines)
                _source.Write(line.Key, line.Value);
        }
        else
        {
            _stream = File.Open(filepath, FileMode.OpenOrCreate);
            _lines = new();
        }
    }

    public TValue this[TId id]
    {
        get => Read(id);
        set => Write(id, value);
    }

    public void Write(TId id, TValue value)
    {
        _lines[id] = value;
        _source.Write(id, value);
    }
    public TValue Read(TId id)
    {
        var value = _source.Read(id);
        Console.WriteLine($"FileRead [{id}] => [{_lines[id]}]");

        return value;
    }
    public bool Contains(TId id)
    {
        var value = _source.Contains(id);
        Console.WriteLine($"FileContains [{id}] => [{_lines.ContainsKey(id)}]");

        return value;
    }

    public void Flush()
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new TypeAwareConverter());
        
        var json = JsonConvert.SerializeObject(_lines, settings);
        _stream.Position = 0;
        var writer = new StreamWriter(_stream);
        writer.Write(json);
        writer.Flush();

        // using (var bsonWriter = new BsonWriter(_stream))
        // {
        //     var serializer = new JsonSerializer();
        //     serializer.Converters.Add(new TypeAwareConverter());
        //     serializer.Serialize(bsonWriter, _lines);
        // }

        _stream.Flush();
        _stream.Close();
    }
}

static class Program
{
    public static FileRepository<TId, TValue> CreateRepo<TId, TValue>(string filename)
    {
        return new FileRepository<TId, TValue>(LoggerRepository<TId, TValue>.Create(DictionaryReadWrite<TId, TValue>.Create()),
            Path.Combine(Environment.CurrentDirectory, filename));
    }

    public static void Main(string[] args)
    {
        var gameConfig = new ServiceCollection();

        // Values ValueId:Value
        var valuesRepo = CreateRepo<int, object>("Values.txt");
        gameConfig.AddStorage(() => valuesRepo);
        // ContractSlots ContractId:[SlotId:ValueId]
        var contractSlotsRepo = CreateRepo<int, int>("Contracts.txt");
        gameConfig.AddContractSlotsInMemory<int, int, int>(() => contractSlotsRepo);
        // Objects ObjectId:[ContractType:ContractId]
        var objectsRepo = CreateRepo<Type, int>("Objects.txt");
        gameConfig.AddObjectsInMemory<int, int>(() => objectsRepo);


        var gameServices = gameConfig.BuildServiceProvider();


        var compositeResolver = new CompositeContractResolverChain();
        compositeResolver.Add(new ContractResolverFromDescriptor(compositeResolver))
            .Add(new ContractResolverFromContainer(gameServices))
            .Add(new ValueResolverFromStorage<int, int, int>(compositeResolver, new IncrementalGenerator<int>()))
            .Add(new ContractResolverFromConstructor<int, int>(compositeResolver, new IncrementalGenerator<int>()));

        var objectContractResolver = new ObjectContractResolver(compositeResolver, gameConfig);
        var objectFactory = new DefaultObjectFactory<int>(new IncrementalGenerator<int>(), objectContractResolver);


        var snakeTemplate = new ObjectTemplateBuilder()
            .Add<ITestContract, TestContract>()
            .Build();

        var snakeBuilder = new ObjectBuilder(objectFactory, snakeTemplate);
        for (var i = 0; i < 3; i++)
        {
            var snake = snakeBuilder.Build();

            var testContract = snake.Get<ITestContract>();

            Console.WriteLine("");
            Console.WriteLine(snake);
            Console.WriteLine(testContract);

            Console.WriteLine("");
            Console.WriteLine("Before");
            //Console.WriteLine(testContract.Number1.Value);
            //Console.WriteLine(testContract.Number2.Value);


            testContract.Number1.Value = i;
            testContract.Number2.Value = i * 2;

            Console.WriteLine("");
            Console.WriteLine("After");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);
        }

        valuesRepo.Flush();
        contractSlotsRepo.Flush();
        objectsRepo.Flush();

        Console.ReadKey(true);
    }
}

public class TypeAwareConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsAssignableTo(typeof(IDictionary))
            && objectType.GenericTypeArguments[1] == typeof(object);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dictionary = (Dictionary<int, object>)value;

        writer.WriteStartObject();
        foreach (var kvp in dictionary)
        {
            writer.WritePropertyName(kvp.Key.ToString());

            var objType = kvp.Value.GetType();
            writer.WriteStartObject();
            writer.WritePropertyName("Type");
            writer.WriteValue(objType.FullName);
            writer.WritePropertyName("Value");
            serializer.Serialize(writer, kvp.Value);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var result = new Dictionary<int, object>();

        var obj = JObject.Load(reader);
        foreach (var property in obj.Properties())
        {
            var key = int.Parse(property.Name);
            var valueObject = (JObject)property.Value;

            var typeName = valueObject["Type"].ToString();
            var valueType = Type.GetType(typeName);
            Console.WriteLine(valueType);

            var value = valueObject["Value"].ToObject(valueType, serializer);

            result[key] = value;
        }

        return result;
    }
}