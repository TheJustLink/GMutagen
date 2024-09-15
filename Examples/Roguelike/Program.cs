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

    public void Dispose()
    {
        Console.WriteLine("Dispose save " + this);

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
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);


            testContract.Number1.Value += i;
            testContract.Number2.Value += i * 2;

            Console.WriteLine("");
            Console.WriteLine("After");
            Console.WriteLine(testContract.Number1.Value);
            Console.WriteLine(testContract.Number2.Value);
        }

        gameServices.Dispose();

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
            serializer.Serialize(writer, objType);
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

            var valueType = valueObject["Type"]!.ToObject<Type>();

            var value = valueObject["Value"]!.ToObject(valueType, serializer)!;

            result[key] = value;
        }

        return result;
    }
}
public class SimpleTypeDictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsAssignableTo(typeof(IDictionary))
            && objectType.GenericTypeArguments[0] == typeof(Type);
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dictionary = (IDictionary)value;

        writer.WriteStartObject();
        foreach (DictionaryEntry entry in dictionary)
        {
            writer.WritePropertyName("Key");
            serializer.Serialize(writer, entry.Key);
            writer.WritePropertyName("Value");
            serializer.Serialize(writer, entry.Value);
        }
        writer.WriteEndObject();
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var valueType = objectType.GenericTypeArguments[1];
        var dictionary = (Activator.CreateInstance(objectType) as IDictionary)!;

        if (reader.TokenType != JsonToken.StartObject) return dictionary;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject) break;
            if (reader.TokenType == JsonToken.PropertyName)
            {
                reader.Skip();
                var key = serializer.Deserialize<Type>(reader)!;
                reader.Read();
                reader.Skip();
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