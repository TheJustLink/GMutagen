using System;
using System.Collections.Generic;
using System.IO;

using GMutagen.v8.IO;

using Newtonsoft.Json;

namespace Roguelike;

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

            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };

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

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

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