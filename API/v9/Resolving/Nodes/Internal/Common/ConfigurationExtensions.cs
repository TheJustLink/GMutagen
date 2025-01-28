using System.IO;
using System.Text.Json;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public static class ConfigurationExtensions
{
    public static IConfiguration<TTargetKey, TTargetValue> ToConfiguration<TTargetKey, TTargetValue>(
        this ConfigurationSection source)
        where TTargetKey : notnull
    {
        return new ConfigurationProxy<TTargetKey, TTargetValue>(source);
    }

    public static IConfiguration<string, string> LoadFromJsonFile(string filePath)
    {
        var rootSection = new ConfigurationSection();
        var json = File.ReadAllText(filePath);
        var parsedData = JsonSerializer.Deserialize<JsonElement>(json);

        if (parsedData.ValueKind == JsonValueKind.Object)
        {
            foreach (var section in parsedData.EnumerateObject())
            {
                ParseSection(section, rootSection);
            }
        }

        return rootSection;
    }

    private static void ParseSection(JsonProperty section, IConfiguration<string, string> configSection)
    {
        if (section.Value.ValueKind == JsonValueKind.Object)
        {
            foreach (var subSection in section.Value.EnumerateObject())
            {
                var childSection = configSection.GetSubSection(subSection.Name);
                ParseSection(subSection, childSection);
            }
        }
        else if (section.Value.ValueKind == JsonValueKind.String)
        {
            configSection[section.Name] = section.Value.GetString();
        }
    }
}