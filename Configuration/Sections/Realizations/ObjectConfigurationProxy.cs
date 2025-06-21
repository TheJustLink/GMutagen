using Configuration.Sections.Interfaces;

namespace Configuration.Sections.Realizations;

public class ObjectConfigurationProxy<TTargetKey, TTargetValue> : IConfiguration<TTargetKey, TTargetValue>
    where TTargetKey : notnull
{
    private const string READ_ONLY_ERROR_MESSAGE = "Proxy configuration is read-only";
    
    private readonly IConfiguration<string, object> _sourceConfiguration;

    public ObjectConfigurationProxy(IConfiguration<string, object> sourceConfiguration)
    {
        _sourceConfiguration = sourceConfiguration ?? throw new ArgumentNullException(nameof(sourceConfiguration));
    }

    public TTargetValue? this[TTargetKey key]
    {
        get => TryGetValue(key, out var value) ? value : default;
        set => throw new NotSupportedException(READ_ONLY_ERROR_MESSAGE);
    }

    public bool TryGetValue(TTargetKey key, out TTargetValue? value)
    {
        value = default;
        
        if (!TryConvertKeyToString(key, out var stringKey))
            return false;

        if (!_sourceConfiguration.TryGetValue(stringKey, out var sourceValue))
            return false;

        return TryConvertValue(sourceValue, out value);
    }

    public bool TryGetValue(string path, out TTargetValue? value)
    {
        value = default;
        
        if (!_sourceConfiguration.TryGetValue(path, out var sourceValue))
            return false;

        return TryConvertValue(sourceValue, out value);
    }

    public IConfiguration<TTargetKey, TTargetValue> Add(TTargetKey key, TTargetValue? value)
        => throw new NotSupportedException(READ_ONLY_ERROR_MESSAGE);

    public IConfiguration<TTargetKey, TTargetValue> AddSection(TTargetKey key, IConfiguration<TTargetKey, TTargetValue> section)
        => throw new NotSupportedException(READ_ONLY_ERROR_MESSAGE);

    public IConfiguration<TTargetKey, TTargetValue> Remove(TTargetKey key)
        => throw new NotSupportedException(READ_ONLY_ERROR_MESSAGE);

    public IEnumerable<KeyValuePair<TTargetKey, TTargetValue?>> GetAllSettings()
    {
        return _sourceConfiguration
            .GetAllSettings()
            .Where(CanConvertSetting)
            .Select(ConvertSetting)
            .Where(setting => setting.HasValue)
            .Select(setting => setting.Value);
    }

    public IConfiguration<TTargetKey, TTargetValue> GetSubSection(TTargetKey subsectionName)
    {
        if (!TryConvertKeyToString(subsectionName, out var stringKey))
            return CreateEmptyProxy();

        if (!_sourceConfiguration.TryGetSubSection(stringKey, out var sourceSubsection))
            return CreateEmptyProxy();

        return new ObjectConfigurationProxy<TTargetKey, TTargetValue>(sourceSubsection);
    }

    public bool TryGetSubSection(TTargetKey subsectionName, out IConfiguration<TTargetKey, TTargetValue> subsection)
    {
        subsection = GetSubSection(subsectionName);
        return !IsEmptyProxy(subsection);
    }

    public IEnumerable<TTargetKey> GetAllSubSectionNames()
    {
        return _sourceConfiguration
            .GetAllSubSectionNames()
            .Where(CanConvertToTargetKey)
            .Select(ConvertToTargetKey)
            .Where(key => key is not null)
            .Cast<TTargetKey>();
    }

    private bool TryConvertKeyToString(TTargetKey key, out string stringKey)
    {
        if (key is string str)
        {
            stringKey = str;
            return true;
        }

        stringKey = string.Empty;
        return false;
    }

    private bool TryConvertValue(object? sourceValue, out TTargetValue? targetValue)
    {
        targetValue = default;
        
        if (sourceValue is null)
            return false;

        try
        {
            if (sourceValue is TTargetValue directValue)
            {
                targetValue = directValue;
                return true;
            }

            targetValue = (TTargetValue)Convert.ChangeType(sourceValue, typeof(TTargetValue));
            return true;
        }
        catch (InvalidCastException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private bool CanConvertSetting(KeyValuePair<string, object> setting)
    {
        return CanConvertToTargetKey(setting.Key);
    }

    private KeyValuePair<TTargetKey, TTargetValue?>? ConvertSetting(KeyValuePair<string, object> setting)
    {
        var targetKey = ConvertToTargetKey(setting.Key);
        if (targetKey is null)
            return null;

        if (!TryConvertValue(setting.Value, out var targetValue))
            return null;

        return new KeyValuePair<TTargetKey, TTargetValue?>(targetKey, targetValue);
    }

    private bool CanConvertToTargetKey(string key)
    {
        return key is TTargetKey;
    }

    private TTargetKey? ConvertToTargetKey(string key)
    {
        return key is TTargetKey targetKey ? targetKey : default;
    }

    private IConfiguration<TTargetKey, TTargetValue> CreateEmptyProxy()
    {
        return new EmptyConfiguration<TTargetKey, TTargetValue>();
    }

    private bool IsEmptyProxy(IConfiguration<TTargetKey, TTargetValue> configuration)
    {
        return configuration is EmptyConfiguration<TTargetKey, TTargetValue>;
    }
}