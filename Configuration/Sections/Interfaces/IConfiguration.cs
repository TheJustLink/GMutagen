namespace Configuration.Sections.Interfaces;

public interface IConfiguration<TKey, TValue>
{
    TValue this[TKey key] { get; set; }
    bool TryGetValue(TKey key, out TValue? value);
    IConfiguration<TKey, TValue> Add(TKey key, TValue value);
    IConfiguration<TKey, TValue> AddSection(TKey key, IConfiguration<TKey, TValue> section);
    IConfiguration<TKey, TValue> Remove(TKey key);
    IEnumerable<KeyValuePair<TKey, TValue>> GetAllSettings();
    IConfiguration<TKey, TValue> GetSubSection(TKey subsectionName);
    bool TryGetSubSection(TKey subsectionName, out IConfiguration<TKey, TValue>? subsection);
    IEnumerable<TKey> GetAllSubSectionNames();
}