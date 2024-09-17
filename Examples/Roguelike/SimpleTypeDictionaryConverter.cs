using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Roguelike;

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