using Configuration.Common;
using Configuration.Sections.Interfaces;

namespace Configuration.Sections.Realizations;

public class FlattenedStringConfiguration<TValue> : IConfiguration<string, TValue>
{
    private readonly Dictionary<string, TValue?> _settings = new();
    private readonly string _separator;
    private readonly string _currentPath;

    public FlattenedStringConfiguration(string separator = ConfigurationPathConstants.PATH_DELIMITER)
        : this(new Dictionary<string, TValue?>(), separator, string.Empty)
    {
    }

    private FlattenedStringConfiguration(Dictionary<string, TValue?> settings, string separator, string currentPath)
    {
        _settings = settings;
        _separator = separator;
        _currentPath = currentPath;
    }

    public TValue? this[string key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public bool TryGetValue(string key, out TValue? value) =>
        _settings.TryGetValue(CreateFullPath(key), out value);

    public IConfiguration<string, TValue> Add(string key, TValue? value)
    {
        SetValue(key, value);
        return this;
    }

    public IConfiguration<string, TValue> AddSection(string key, IConfiguration<string, TValue> section)
    {
        var sectionPath = CreateFullPath(key);
        FlattenSectionRecursively(section, sectionPath);
        return CreateSubConfiguration(sectionPath);
    }

    public IConfiguration<string, TValue> Remove(string key)
    {
        var pathToRemove = CreateFullPath(key);
        RemovePathAndChildren(pathToRemove);
        return this;
    }

    public IEnumerable<KeyValuePair<string, TValue?>> GetAllSettings()
    {
        var pathPrefix = CreatePathPrefix();
        
        return _settings
            .Where(kvp => IsInCurrentPath(kvp.Key, pathPrefix))
            .Where(kvp => IsDirectChild(kvp.Key, pathPrefix))
            .Select(kvp => CreateKeyValuePair(kvp, pathPrefix));
    }

    public IConfiguration<string, TValue> GetSubSection(string subsectionName)
    {
        var subsectionPath = CreateFullPath(subsectionName);
        return CreateSubConfiguration(subsectionPath);
    }

    public bool TryGetSubSection(string subsectionName, out IConfiguration<string, TValue> subsection)
    {
        var subsectionPath = CreateFullPath(subsectionName);
        
        if (HasChildPaths(subsectionPath))
        {
            subsection = CreateSubConfiguration(subsectionPath);
            return true;
        }

        subsection = null!;
        return false;
    }

    public IEnumerable<string> GetAllSubSectionNames()
    {
        var pathPrefix = CreatePathPrefix();
        var subsectionNames = new HashSet<string>();

        foreach (var key in _settings.Keys)
        {
            if (TryExtractSubsectionName(key, pathPrefix, out var subsectionName))
            {
                subsectionNames.Add(subsectionName);
            }
        }

        return subsectionNames;
    }

    public IEnumerable<KeyValuePair<string, TValue?>> GetAllFlatSettings() => _settings;

    public void ImportFromHierarchical(IConfiguration<string, TValue> hierarchicalConfig)
    {
        FlattenSectionRecursively(hierarchicalConfig, string.Empty);
    }

    public bool TryGetValueByPath(string path, out TValue? value) =>
        _settings.TryGetValue(path, out value);

    public void SetValueByPath(string path, TValue? value) =>
        _settings[path] = value;

    public void Clear() => _settings.Clear();

    public int Count => _settings.Count;

    private TValue? GetValue(string key)
    {
        var fullPath = CreateFullPath(key);
        return _settings.TryGetValue(fullPath, out var value)
            ? value
            : throw new KeyNotFoundException($"Key '{key}' not found at path '{_currentPath}'");
    }

    private void SetValue(string key, TValue? value)
    {
        var fullPath = CreateFullPath(key);
        _settings[fullPath] = value;
    }

    private string CreateFullPath(string key) =>
        IsRootPath() ? key : $"{_currentPath}{_separator}{key}";

    private string CreatePathPrefix() =>
        IsRootPath() ? string.Empty : $"{_currentPath}{_separator}";

    private bool IsRootPath() => string.IsNullOrEmpty(_currentPath);

    private FlattenedStringConfiguration<TValue> CreateSubConfiguration(string path) =>
        new(_settings, _separator, path);

    private void FlattenSectionRecursively(IConfiguration<string, TValue> section, string basePath)
    {
        AddAllSettingsFromSection(section, basePath);
        AddAllSubsectionsFromSection(section, basePath);
    }

    private void AddAllSettingsFromSection(IConfiguration<string, TValue> section, string basePath)
    {
        foreach (var setting in section.GetAllSettings())
        {
            var settingPath = CombinePaths(basePath, setting.Key);
            _settings[settingPath] = setting.Value;
        }
    }

    private void AddAllSubsectionsFromSection(IConfiguration<string, TValue> section, string basePath)
    {
        foreach (var subsectionName in section.GetAllSubSectionNames())
        {
            if (section.TryGetSubSection(subsectionName, out var subsection))
            {
                var subsectionPath = CombinePaths(basePath, subsectionName);
                FlattenSectionRecursively(subsection, subsectionPath);
            }
        }
    }

    private string CombinePaths(string basePath, string key) =>
        string.IsNullOrEmpty(basePath) ? key : $"{basePath}{_separator}{key}";

    private void RemovePathAndChildren(string pathToRemove)
    {
        _settings.Remove(pathToRemove);
        
        var childPrefix = $"{pathToRemove}{_separator}";
        var childPaths = _settings.Keys
            .Where(key => key.StartsWith(childPrefix))
            .ToList();

        foreach (var childPath in childPaths)
        {
            _settings.Remove(childPath);
        }
    }

    private bool IsInCurrentPath(string key, string pathPrefix) =>
        key.StartsWith(pathPrefix);

    private bool IsDirectChild(string key, string pathPrefix)
    {
        var relativePath = key.Substring(pathPrefix.Length);
        return !relativePath.Contains(_separator);
    }

    private KeyValuePair<string, TValue?> CreateKeyValuePair(KeyValuePair<string, TValue?> kvp, string pathPrefix)
    {
        var relativeKey = kvp.Key.Substring(pathPrefix.Length);
        return new KeyValuePair<string, TValue?>(relativeKey, kvp.Value);
    }

    private bool HasChildPaths(string parentPath)
    {
        var childPrefix = $"{parentPath}{_separator}";
        return _settings.Keys.Any(key => key.StartsWith(childPrefix));
    }

    private bool TryExtractSubsectionName(string key, string pathPrefix, out string subsectionName)
    {
        subsectionName = string.Empty;
        
        if (!key.StartsWith(pathPrefix))
            return false;

        var relativePath = key.Substring(pathPrefix.Length);
        var pathParts = relativePath.Split(new[] { _separator }, StringSplitOptions.RemoveEmptyEntries);
        
        if (pathParts.Length <= 1)
            return false;

        subsectionName = pathParts[0];
        return true;
    }
}