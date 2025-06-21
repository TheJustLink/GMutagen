using Serialization.Attributes;

namespace Serialization.Models;

public class ConfigurationMetaData
{
    [Serialize] public string? Id { get; set; } = string.Empty;
}