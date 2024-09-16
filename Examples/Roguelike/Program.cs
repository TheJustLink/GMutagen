using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using GMutagen.v8.IO;
using GMutagen.v8.Values;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using GMutagen.v8.Generators;
using GMutagen.v8.Contracts.Resolving;
using GMutagen.v8.Contracts.Resolving.Attributes;
using GMutagen.v8.Contracts.Resolving.Nodes;
using GMutagen.v8.Objects;
using GMutagen.v8.Objects.Templates;
using GMutagen.v8.Contracts;
using GMutagen.v8.IO.Sources.Dictionary;
using System.Linq;

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
public class LoggerRepository<TId, TValue> : IReadWrite<TId, TValue> where TId : notnull
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
public class FileRepository<TId, TValue> : IReadWrite<TId, TValue>, IDisposable
    where TId : notnull
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
            settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            settings.TypeNameHandling = TypeNameHandling.None;
            settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
            settings.Converters.Add(new SimpleTypeConverter());
            settings.Converters.Add(new TypeAwareConverter());
            settings.Converters.Add(new SimpleTypeDictionaryConverter());

            Console.WriteLine($"Loading save {typeof(TId).Name}:{typeof(TValue).Name}");
            _lines = JsonConvert.DeserializeObject<Dictionary<TId, TValue>>(reader.ReadToEnd(), settings) ?? new();
            
            foreach (var line in _lines)
                _source.Write(line.Key, line.Value);
            Console.WriteLine($"Save {typeof(TId).Name}:{typeof(TValue).Name} loaded");
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
    public TValue Read(TId id) => _source.Read(id);
    public bool Contains(TId id) => _source.Contains(id);

    public void Dispose()
    {
        Console.WriteLine($"Dispose save {typeof(TId).Name}:{typeof(TValue).Name}");

        var settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.None;
        settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
        settings.Converters.Add(new SimpleTypeConverter());
        settings.Converters.Add(new TypeAwareConverter());
        settings.Converters.Add(new SimpleTypeDictionaryConverter());

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
public class FileReadWriteFactory : IReadWriteFactory
{
    private readonly IReadWriteFactory _sourceStorageFactory;
    private readonly string _defaultFilePath;
    public FileReadWriteFactory(IReadWriteFactory sourceStorageFactory, string defaultFilePath)
    {
        _sourceStorageFactory = sourceStorageFactory;
        _defaultFilePath = defaultFilePath;
    }

    public IReadWrite<TId, TValue> CreateReadWrite<TId, TValue>() where TId : notnull
    {
        var source = _sourceStorageFactory.CreateReadWrite<TId, TValue>();

        return new FileRepository<TId, TValue>(source, _defaultFilePath);
    }
    public IReadWrite<TId, TValue> Create<TId, TValue>(string filePath) where TId : notnull
    {
        var source = _sourceStorageFactory.CreateReadWrite<TId, TValue>();

        return new FileRepository<TId, TValue>(source, filePath);
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
            .Add(new ValueResolverFromStorage<int, int, int>(gameServices, compositeResolver, new IncrementalGenerator<int>()))
            .Add(new ContractResolverFromConstructor<int, int>(gameServices, compositeResolver, new IncrementalGenerator<int>()));

        var objectFactory = new ResolvingObjectFactory<int>(new IncrementalGenerator<int>(), compositeResolver);


        var snakeTemplate = new ObjectTemplateBuilder()
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

        godContract.Number1.Value += 1;
        godContract.Number2.Value += 2;

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

public class TypeAwareConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.GetInterfaces().Any(i => i.IsGenericType
            && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
            && i.GenericTypeArguments[1] == typeof(object));
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dictionary = (IDictionary)value;

        writer.WriteStartObject();
        foreach (DictionaryEntry entry in dictionary)
        {
            writer.WritePropertyName(entry.Key.ToString()!);
            writer.WriteStartObject();
            writer.WritePropertyName(entry.Value!.GetType().FullName!);
            serializer.Serialize(writer, entry.Value);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var keyType = objectType.GenericTypeArguments[0];
        var dictionary = (Activator.CreateInstance(objectType) as IDictionary)!;

        if (reader.TokenType != JsonToken.StartObject) return dictionary;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject) break;
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var key = Convert.ChangeType(reader.Value!, keyType);
                reader.Read();
                
                reader.Read();
                var typeFullName = (reader.Value as string)!;
                var valueType = Type.GetType(typeFullName)!;
                
                reader.Read();
                var value = serializer.Deserialize(reader, valueType)!;

                reader.Read();
                dictionary[key] = value;
            }
        }

        return dictionary;
    }
}
public class SimpleTypeDictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.GetInterfaces().Any(i => i.IsGenericType
            && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
            && i.GenericTypeArguments[0] == typeof(Type));
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dictionary = (IDictionary)value;

        writer.WriteStartObject();
        foreach (DictionaryEntry entry in dictionary)
        {
            writer.WritePropertyName((entry.Key as Type)!.FullName!);
            serializer.Serialize(writer, entry.Value);
        }
        writer.WriteEndObject();
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var valueType = objectType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GenericTypeArguments[1];
        var dictionary = (Activator.CreateInstance(objectType) as IDictionary)!;

        if (reader.TokenType != JsonToken.StartObject) return dictionary;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject) break;
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var typeFullName = (reader.Value as string)!;
                var key = Type.GetType(typeFullName)!;

                reader.Read();
                var value = serializer.Deserialize(reader, valueType)!;

                dictionary[key] = value;
            }
        }

        return dictionary;
    }
}
public class SimpleTypeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsAssignableTo(typeof(Type));
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Type)value).FullName);
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Type.GetType((string)reader.Value!)!;
    }
}