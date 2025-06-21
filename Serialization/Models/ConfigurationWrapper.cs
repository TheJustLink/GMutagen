using Serialization.Attributes;

namespace Serialization.Models;

public class ConfigurationWrapper<T>
{
    [Serialize] public ConfigurationMetaData MetaData { get; set; } = new();
    [Serialize] public T Configuration { get; set; } = default!;
}