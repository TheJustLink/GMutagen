using Configuration.Sections.Interfaces;

namespace Configuration.Sections.Realizations;

public class FlattenedConfiguration<TKey, TValue> : IConfiguration<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<Path<TKey>, TValue?> _settings = new();
    private readonly Path<TKey> _currentPath;

    public FlattenedConfiguration() : this(new Dictionary<Path<TKey>, TValue?>(), new Path<TKey>())
    {
    }

    private FlattenedConfiguration(Dictionary<Path<TKey>, TValue?> settings, Path<TKey> currentPath)
    {
        _settings = settings;
        _currentPath = currentPath;
    }

    public TValue? this[TKey key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public bool TryGetValue(TKey key, out TValue? value) =>
        _settings.TryGetValue(CreateFullPath(key), out value);

    public IConfiguration<TKey, TValue> Add(TKey key, TValue? value)
    {
        SetValue(key, value);
        return this;
    }

    public IConfiguration<TKey, TValue> AddSection(TKey key, IConfiguration<TKey, TValue> section)
    {
        var sectionPath = CreateFullPath(key);
        FlattenSectionRecursively(section, sectionPath);
        return CreateSubConfiguration(sectionPath);
    }

    public IConfiguration<TKey, TValue> Remove(TKey key)
    {
        var pathToRemove = CreateFullPath(key);
        RemovePathAndChildren(pathToRemove);
        return this;
    }

    public IEnumerable<KeyValuePair<TKey, TValue?>> GetAllSettings()
    {
        var currentDepth = _currentPath.Depth;
        
        return _settings
            .Where(kvp => IsDirectChild(kvp.Key, currentDepth))
            .Select(kvp => new KeyValuePair<TKey, TValue?>(kvp.Key.LastSegment, kvp.Value));
    }

    public IConfiguration<TKey, TValue> GetSubSection(TKey subsectionName)
    {
        var subsectionPath = CreateFullPath(subsectionName);
        return CreateSubConfiguration(subsectionPath);
    }

    public bool TryGetSubSection(TKey subsectionName, out IConfiguration<TKey, TValue> subsection)
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

    public IEnumerable<TKey> GetAllSubSectionNames()
    {
        var currentDepth = _currentPath.Depth;

        return _settings.Keys
            .Where(path => HasSubsections(path, currentDepth))
            .Select(path => path.GetSegmentAt(currentDepth))
            .Distinct();
    }

    public IEnumerable<KeyValuePair<Path<TKey>, TValue?>> GetAllFlatSettings() => _settings;

    public void ImportFromHierarchical(IConfiguration<TKey, TValue> hierarchicalConfig)
    {
        FlattenSectionRecursively(hierarchicalConfig, Path<TKey>.Empty);
    }

    public bool TryGetValueByPath(Path<TKey> path, out TValue? value) =>
        _settings.TryGetValue(path, out value);

    public void SetValueByPath(Path<TKey> path, TValue? value) =>
        _settings[path] = value;

    public void Clear() => _settings.Clear();

    public int Count => _settings.Count;

    private TValue? GetValue(TKey key)
    {
        var fullPath = CreateFullPath(key);
        return _settings.TryGetValue(fullPath, out var value)
            ? value
            : throw new KeyNotFoundException($"Key '{key}' not found at path {_currentPath}");
    }

    private void SetValue(TKey key, TValue? value)
    {
        var fullPath = CreateFullPath(key);
        _settings[fullPath] = value;
    }

    private Path<TKey> CreateFullPath(TKey key) => _currentPath.Append(key);

    private FlattenedConfiguration<TKey, TValue> CreateSubConfiguration(Path<TKey> path) =>
        new(_settings, path);

    private void FlattenSectionRecursively(IConfiguration<TKey, TValue> section, Path<TKey> basePath)
    {
        AddAllSettingsFromSection(section, basePath);
        AddAllSubsectionsFromSection(section, basePath);
    }

    private void AddAllSettingsFromSection(IConfiguration<TKey, TValue> section, Path<TKey> basePath)
    {
        foreach (var setting in section.GetAllSettings())
        {
            var settingPath = basePath.Append(setting.Key);
            _settings[settingPath] = setting.Value;
        }
    }

    private void AddAllSubsectionsFromSection(IConfiguration<TKey, TValue> section, Path<TKey> basePath)
    {
        foreach (var subsectionName in section.GetAllSubSectionNames())
        {
            if (section.TryGetSubSection(subsectionName, out var subsection))
            {
                var subsectionPath = basePath.Append(subsectionName);
                FlattenSectionRecursively(subsection, subsectionPath);
            }
        }
    }

    private void RemovePathAndChildren(Path<TKey> pathToRemove)
    {
        _settings.Remove(pathToRemove);
        
        var childPaths = _settings.Keys
            .Where(path => path.IsChildOf(pathToRemove))
            .ToList();

        foreach (var childPath in childPaths)
        {
            _settings.Remove(childPath);
        }
    }

    private bool IsDirectChild(Path<TKey> path, int currentDepth)
    {
        return path.Depth == currentDepth + 1 && 
               path.StartsWithPath(_currentPath);
    }

    private bool HasChildPaths(Path<TKey> parentPath) =>
        _settings.Keys.Any(path => path.IsChildOf(parentPath));

    private bool HasSubsections(Path<TKey> path, int currentDepth)
    {
        return path.Depth > currentDepth + 1 && 
               path.StartsWithPath(_currentPath);
    }
}