using System;

using Newtonsoft.Json;

namespace Roguelike;

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