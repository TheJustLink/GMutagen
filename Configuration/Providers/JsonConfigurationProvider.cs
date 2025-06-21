using System.Text.Json;
using Configuration.Common;
using Configuration.Providers.Interfaces;
using Configuration.Sections.Interfaces;
using Configuration.Sections.Realizations;

namespace Configuration.Providers;

public class JsonObjectConfigurationProvider : IConfigurationProvider
{
    private readonly string _filePath;
    private readonly bool _reloadOnChange;
    private readonly object _lock = new();
    private readonly IConfiguration<string, string> _root;
    private readonly HashSet<string> _ownedKeys = new();
    
    private FileSystemWatcher? _watcher;

    public string Name { get; }
    public int Priority { get; }
    public event Action ConfigurationChanged;

    public JsonObjectConfigurationProvider(
        string filePath, 
        IConfiguration<string, string> root,
        FileSystemWatcher? watcher = null, 
        bool reloadOnChange = false, 
        string? name = null, 
        int priority = 0)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _root = root ?? throw new ArgumentNullException(nameof(root));
        _watcher = watcher;
        _reloadOnChange = reloadOnChange;
        Name = name ?? Path.GetFileName(filePath);
        Priority = priority;

        LoadConfiguration();
        InitializeFileWatcher();
    }

    public void Load()
    {
        lock (_lock)
        {
            LoadConfiguration();
        }
    }

    public void Unload()
    {
        ClearOwnedKeys();
    }

    public void Dispose()
    {
        Unload();
        _watcher?.Dispose();
    }

    private void LoadConfiguration()
    {
        ValidateFileExists();
        var jsonContent = File.ReadAllText(_filePath);
        var document = JsonDocument.Parse(jsonContent);
        ParseJsonElement(document.RootElement, _root, string.Empty);
    }

    private void ValidateFileExists()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("Configuration file not found.", _filePath);
    }

    private void ParseJsonElement(JsonElement element, IConfiguration<string, string> section, string currentPath)
    {
        foreach (var property in element.EnumerateObject())
        {
            var fullPath = BuildFullPath(currentPath, property.Name);
            var value = ParseJsonValue(property.Value, fullPath);
            
            RegisterKey(fullPath);
            AddToSection(section, property.Name, value);
        }
    }

    private string BuildFullPath(string currentPath, string propertyName)
    {
        return string.IsNullOrEmpty(currentPath) 
            ? propertyName 
            : $"{currentPath}{ConfigurationPathConstants.PATH_DELIMITER}{propertyName}";
    }

    private object ParseJsonValue(JsonElement element, string currentPath)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => CreateSubSection(element, currentPath),
            _ => element.GetRawText()
        };
    }

    private IConfiguration<string, string> CreateSubSection(JsonElement element, string currentPath)
    {
        var section = CreateSectionInstance();
        ParseJsonElement(element, section, currentPath);
        return section;
    }

    private IConfiguration<string, string> CreateSectionInstance()
    {
        return _root is FlattenedStringConfiguration<string> 
            ? new DefaultFlattenedConfiguration() 
            : new DefaultConfigurationSection();
    }

    private void RegisterKey(string key)
    {
        _ownedKeys.Add(key);
    }

    private void AddToSection(IConfiguration<string, string> section, string key, object value)
    {
        if (value is IConfiguration<string, string> subSection)
        {
            section.AddSection(key, subSection);
        }
        else
        {
            section.Add(key, (string)value);
        }
    }

    private void ClearOwnedKeys()
    {
        foreach (var key in _ownedKeys)
        {
            _root.Remove(key);
        }
        _ownedKeys.Clear();
    }

    private void InitializeFileWatcher()
    {
        if (!_reloadOnChange) return;

        SetupFileWatcher();
        StartWatching();
    }

    private void SetupFileWatcher()
    {
        if (_watcher != null) return;

        var directory = Path.GetDirectoryName(_filePath);
        if (directory == null) return;

        _watcher = new FileSystemWatcher(directory)
        {
            Filter = Path.GetFileName(_filePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
    }

    private void StartWatching()
    {
        if (_watcher == null) return;

        _watcher.Changed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Task.Delay(200).ContinueWith(_ => ReloadConfiguration());
    }

    private void ReloadConfiguration()
    {
        Unload();
        Load();
    }
}