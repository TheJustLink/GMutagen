using System;
using System.Collections.Generic;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public class ConfigurationSection : IConfiguration<string, string>
{
    private readonly Dictionary<string, string> _settings = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, IConfiguration<string, string>> _subSections =
        new(StringComparer.OrdinalIgnoreCase);

    public string this[string key]
    {
        get => _settings.ContainsKey(key) ? _settings[key] : null;
        set => _settings[key] = value;
    }

    public bool TryGetValue(string path, out string value)
    {
        value = null;
        var keys = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        IConfiguration<string, string> currentSection = this;
        for (int i = 0; i < keys.Length; i++)
        {
            var key = keys[i];

            if (i == keys.Length - 1)
            {
                return currentSection.TryGetValue(key, out value);
            }

            if (!currentSection.TryGetSubSection(key, out var nextSection))
            {
                return false;
            }

            currentSection = nextSection;
        }

        return false;
    }

    public void Add(string key, string value) => _settings[key] = value;

    public void Remove(string key) => _settings.Remove(key);

    public IEnumerable<KeyValuePair<string, string>> GetAllSettings() => _settings;

    public IConfiguration<string, string> GetSubSection(string subsectionName)
    {
        if (!_subSections.ContainsKey(subsectionName))
        {
            _subSections[subsectionName] = new ConfigurationSection();
        }

        return _subSections[subsectionName];
    }

    public bool TryGetSubSection(string subsectionName, out IConfiguration<string, string> subsection) =>
        _subSections.TryGetValue(subsectionName, out subsection);

    public IEnumerable<string> GetAllSubSectionNames() => _subSections.Keys;
}