using System.Text.Json;
using Configuration.Common;
using Configuration.Providers.Interfaces;
using Configuration.Sections.Interfaces;
using Configuration.Sections.Realizations;
using Serialization.Models;

namespace Serialization.Providers;

public class SerializationConfigurationProvider : IConfigurationProvider
{
    private const string NAME_PREFIX = "Serialization";
    private const string NAME_DELIMITER = "_";

    private readonly string _filePath;
    private readonly string _typeName;
    private readonly string _configurationId;
    private readonly bool _reloadOnChange;
    private readonly object _lock = new();
    private readonly HashSet<string> _loadedKeys = new();
    private readonly IConfiguration<string, object> _configuration;

    private FileSystemWatcher? _watcher;
    private bool _disposed;

    public string Name { get; }
    public int Priority { get; }
    public event Action ConfigurationChanged;

    public SerializationConfigurationProvider(
        string filePath, 
        string typeName, 
        string configurationId,
        IConfiguration<string, object> configuration, 
        int priority = 0, 
        bool reloadOnChange = true)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _typeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        _configurationId = configurationId ?? throw new ArgumentNullException(nameof(configurationId));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _reloadOnChange = reloadOnChange;
        Priority = priority;
        Name = CreateProviderName(typeName, configurationId);
    }

    public void Load()
    {
        lock (_lock)
        {
            LoadConfigurationFromFile();
            InitializeFileWatcher();
        }
    }

    public void Unload()
    {
        ClearLoadedKeys();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _watcher?.Dispose();
        _disposed = true;
    }

    private static string CreateProviderName(string typeName, string configurationId)
    {
        return $"{NAME_PREFIX}{NAME_DELIMITER}{typeName}{NAME_DELIMITER}{configurationId}";
    }

    private void LoadConfigurationFromFile()
    {
        if (!File.Exists(_filePath))
        {
            LogFileNotFound();
            return;
        }

        try
        {
            var jsonContent = File.ReadAllText(_filePath);
            var document = JsonDocument.Parse(jsonContent);
            ProcessJsonDocument(document);
        }
        catch (Exception ex)
        {
            LogLoadError(ex);
        }
    }

    private void ProcessJsonDocument(JsonDocument document)
    {
        if (!document.RootElement.TryGetProperty(nameof(ConfigurationWrapper<object>.Configuration), out var configElement))
            return;

        var sectionPath = ConfigurationPathConstants.GetPath(_typeName, _configurationId);
        LoadJsonIntoConfiguration(configElement, sectionPath);
    }

    private void LoadJsonIntoConfiguration(JsonElement element, string basePath)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return;

        foreach (var property in element.EnumerateObject())
        {
            var fullPath = BuildFullPath(basePath, property.Name);
            RegisterLoadedKey(fullPath);
            
            var value = ConvertJsonElementToValue(property.Value);
            SetConfigurationValue(fullPath, value);

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                LoadJsonIntoConfiguration(property.Value, fullPath);
            }
        }
    }

    private string BuildFullPath(string basePath, string propertyName)
    {
        return $"{basePath}{ConfigurationPathConstants.PATH_DELIMITER}{propertyName}";
    }

    private void RegisterLoadedKey(string key)
    {
        _loadedKeys.Add(key);
    }

    private void SetConfigurationValue(string path, object? value)
    {
        if (_configuration is FlattenedStringConfiguration<object> flatConfig)
        {
            flatConfig.SetValueByPath(path, value);
        }
        else
        {
            SetValueInHierarchy(path, value);
        }
    }

    private void SetValueInHierarchy(string path, object? value)
    {
        var keys = path.Split(ConfigurationPathConstants.PATH_DELIMITER, StringSplitOptions.RemoveEmptyEntries);
        var currentSection = _configuration;

        var last = keys.Length - 1;
        for (int i = 0; i < last; i++)
        {
            currentSection = GetOrCreateSection(currentSection, keys[i]);
        }

        currentSection[keys[last]] = value;
    }

    private IConfiguration<string, object> GetOrCreateSection(IConfiguration<string, object> parent, string key)
    {
        if (parent.TryGetSubSection(key, out var existingSection))
            return existingSection;

        var newSection = new ConfigurationSection<string, object>();
        parent.AddSection(key, newSection);
        return newSection;
    }

    private object? ConvertJsonElementToValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => ConvertJsonNumber(element),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => ConvertJsonArray(element),
            _ => element.GetRawText()
        };
    }

    private object ConvertJsonNumber(JsonElement element)
    {
        if (element.TryGetInt32(out var intValue))
            return intValue;
        if (element.TryGetInt64(out var longValue))
            return longValue;
        if (element.TryGetDouble(out var doubleValue))
            return doubleValue;
        return element.GetDecimal();
    }

    private Dictionary<string, object?> ConvertJsonObject(JsonElement element)
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJsonElementToValue(property.Value);
        }
        return dictionary;
    }

    private List<object?> ConvertJsonArray(JsonElement element)
    {
        var list = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            list.Add(ConvertJsonElementToValue(item));
        }
        return list;
    }

    private void ClearLoadedKeys()
    {
        foreach (var key in _loadedKeys)
        {
            _configuration.Remove(key);
        }
        _loadedKeys.Clear();
    }

    private void InitializeFileWatcher()
    {
        if (!_reloadOnChange || _watcher != null)
            return;

        CreateFileWatcher();
    }

    private void CreateFileWatcher()
    {
        var directory = Path.GetDirectoryName(_filePath);
        var fileName = Path.GetFileName(_filePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            return;

        _watcher = new FileSystemWatcher(directory)
        {
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Changed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Task.Delay(200)
            .ContinueWith(_ => HandleFileChange());
    }
    
    private void HandleFileChange()
    {
        ReloadConfiguration();
        ConfigurationChanged?.Invoke();
    }
    
    private void ReloadConfiguration()
    {
        Unload();
        Load();
    }

    private void LogFileNotFound()
    {
        Console.WriteLine($"Configuration file not found: {_filePath}");
    }

    private void LogLoadError(Exception ex)
    {
        Console.WriteLine($"Error loading configuration from {_filePath}: {ex.Message}");
    }
}