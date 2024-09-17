using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Roguelike;

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