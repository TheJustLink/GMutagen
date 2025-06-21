using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public class ConfigurationSection : IConfiguration<string, string>
{
    public const string DELIMITER = ".";
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
        var keys = path.Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);

        if (keys.Length == 1)
        {
            return _settings.TryGetValue(keys[0], out value);
        }

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

    public IConfiguration<string, string> Add(string key, string value)
    {
        _settings[key] = value;
        return this;
    }


    public IConfiguration<string, string> AddSection(string key)
    {
        var newSection = new ConfigurationSection();
        _subSections.Add(key, newSection);
        return newSection;
    }

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

public class JsonConfigurationProvider : IConfiguration<string, string>
{
    private readonly string _filePath;
    private readonly bool _reloadOnChange;
    private FileSystemWatcher _watcher;
    private readonly object _lock = new();
    private ConfigurationSection _root;

    public IConfiguration<string, string> Configuration => _root;

    public JsonConfigurationProvider(string filePath, bool reloadOnChange = false)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _reloadOnChange = reloadOnChange;
        LoadConfiguration();

        if (_reloadOnChange)
        {
            StartFileWatcher();
        }
    }

    private void LoadConfiguration()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException("Configuration file not found.", _filePath);

            var json = File.ReadAllText(_filePath);
            var jsonDoc = JsonDocument.Parse(json);
            _root = new ConfigurationSection();

            ParseElement(jsonDoc.RootElement, _root);
        }
    }

    private void ParseElement(JsonElement element, ConfigurationSection section)
    {
        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                var subSection = section.GetSubSection(prop.Name) as ConfigurationSection;
                ParseElement(prop.Value, subSection);
            }
            else
            {
                section.Add(prop.Name, prop.Value.ToString());
            }
        }
    }

    private void StartFileWatcher()
    {
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_filePath))
        {
            Filter = Path.GetFileName(_filePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Changed += (_, __) =>
        {
            Task.Delay(100).ContinueWith(_ =>
            {
                try
                {
                    LoadConfiguration();
                }
                catch
                {
                    //TODO: log errors
                }
            });
        };

        _watcher.EnableRaisingEvents = true;
    }

    public string this[string key]
    {
        get => _root[key];
        set => _root[key] = value;
    }

    public bool TryGetValue(string key, out string value)
        => _root.TryGetValue(key, out value);

    public IConfiguration<string, string> Add(string key, string value)
        => _root.Add(key, value);

    public IConfiguration<string, string> AddSection(string key)
        => _root.AddSection(key);

    public void Remove(string key)
        => _root.Remove(key);

    public IEnumerable<KeyValuePair<string, string>> GetAllSettings()
        => _root.GetAllSettings();

    public IConfiguration<string, string> GetSubSection(string subsectionName)
        => _root.GetSubSection(subsectionName);

    public bool TryGetSubSection(string subsectionName, out IConfiguration<string, string> subsection)
        => _root.TryGetSubSection(subsectionName, out subsection);

    public IEnumerable<string> GetAllSubSectionNames()
        => _root.GetAllSubSectionNames();
}