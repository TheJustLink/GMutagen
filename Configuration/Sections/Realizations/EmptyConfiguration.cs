using Configuration.Sections.Interfaces;

namespace Configuration.Sections.Realizations;

internal class EmptyConfiguration<TKey, TValue> : IConfiguration<TKey, TValue>
    where TKey : notnull
{
    public TValue? this[TKey key]
    {
        get => default;
        set { }
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        value = default;
        return false;
    }

    public IConfiguration<TKey, TValue> Add(TKey key, TValue? value) => this;

    public IConfiguration<TKey, TValue> AddSection(TKey key, IConfiguration<TKey, TValue> section) => this;

    public IConfiguration<TKey, TValue> Remove(TKey key) => this;

    public IEnumerable<KeyValuePair<TKey, TValue?>> GetAllSettings() => Enumerable.Empty<KeyValuePair<TKey, TValue?>>();

    public IConfiguration<TKey, TValue> GetSubSection(TKey subsectionName) => this;

    public bool TryGetSubSection(TKey subsectionName, out IConfiguration<TKey, TValue> subsection)
    {
        subsection = this;
        return false;
    }

    public IEnumerable<TKey> GetAllSubSectionNames() => Enumerable.Empty<TKey>();
}