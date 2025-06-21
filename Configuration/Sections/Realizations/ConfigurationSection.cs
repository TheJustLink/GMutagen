using Configuration.Sections.Interfaces;

namespace Configuration.Sections.Realizations;

public class ConfigurationSection<TKey, TValue> : IConfiguration<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _settings = new();
    private readonly Dictionary<TKey, IConfiguration<TKey, TValue>> _sections = new();

    public TValue this[TKey key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public bool TryGetValue(TKey key, out TValue? value) => _settings.TryGetValue(key, out value);

    public IConfiguration<TKey, TValue> Add(TKey key, TValue value)
    {
        SetValue(key, value);
        return this;
    }

    public IConfiguration<TKey, TValue> AddSection(TKey key, IConfiguration<TKey, TValue> section)
    {
        _sections[key] = section;
        return section;
    }

    public IConfiguration<TKey, TValue> Remove(TKey key)
    {
        RemoveKey(key);
        return this;
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> GetAllSettings() => _settings;

    public IConfiguration<TKey, TValue> GetSubSection(TKey subsectionName)
    {
        return GetOrCreateSubSection(subsectionName);
    }

    public bool TryGetSubSection(TKey subsectionName, out IConfiguration<TKey, TValue>? subsection)
    {
        return _sections.TryGetValue(subsectionName, out subsection);
    }

    public IEnumerable<TKey> GetAllSubSectionNames() => _sections.Keys;
    private TValue GetValue(TKey key)
    {
        if (_settings.TryGetValue(key, out var value))
            return value;

        throw new KeyNotFoundException($"Key '{key}' not found.");
    }

    private void SetValue(TKey key, TValue value)
    {
        _settings[key] = value;
    }

    private void RemoveKey(TKey key)
    {
        _settings.Remove(key);
        _sections.Remove(key);
    }

    private IConfiguration<TKey, TValue> GetOrCreateSubSection(TKey subsectionName)
    {
        if (_sections.TryGetValue(subsectionName, out var existingSection))
            return existingSection;

        var newSection = CreateNewSection();
        _sections[subsectionName] = newSection;
        return newSection;
    }

    private IConfiguration<TKey, TValue> CreateNewSection()
    {
        return new ConfigurationSection<TKey, TValue>();
    }
}