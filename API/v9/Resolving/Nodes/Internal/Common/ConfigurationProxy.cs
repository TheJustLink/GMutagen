using System;
using System.Collections.Generic;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public class ConfigurationProxy<TTargetKey, TTargetValue> : IConfiguration<TTargetKey, TTargetValue>
    where TTargetKey : notnull
{
    private readonly IConfiguration<string, string> _source;

    public ConfigurationProxy(IConfiguration<string, string> source)
    {
        _source = source;
    }

    public TTargetValue this[TTargetKey key]
    {
        get
        {
            if (key is string sourceKey && _source.TryGetValue(sourceKey, out var sourceValue))
            {
                return (TTargetValue)Convert.ChangeType(sourceValue, typeof(TTargetValue));
            }

            return default;
        }
        set => throw new NotSupportedException("Proxy configuration is read-only.");
    }

    public bool TryGetValue(TTargetKey key, out TTargetValue value)
    {
        if (key is string sourceKey && _source.TryGetValue(sourceKey, out var sourceValue))
        {
            value = (TTargetValue)Convert.ChangeType(sourceValue, typeof(TTargetValue));
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(string path, out TTargetValue value)
    {
        value = default;
        if (_source.TryGetValue(path, out var sourceValue))
        {
            value = (TTargetValue)Convert.ChangeType(sourceValue, typeof(TTargetValue));
            return true;
        }

        return false;
    }

    public void Add(TTargetKey key, TTargetValue value) =>
        throw new NotSupportedException("Proxy configuration is read-only.");

    public void Remove(TTargetKey key) => throw new NotSupportedException("Proxy configuration is read-only.");

    public IEnumerable<KeyValuePair<TTargetKey, TTargetValue>> GetAllSettings()
    {
        foreach (var setting in _source.GetAllSettings())
        {
            if (setting.Key is TTargetKey targetKey && setting.Value is TTargetValue targetValue)
            {
                yield return new KeyValuePair<TTargetKey, TTargetValue>(targetKey, targetValue);
            }
        }
    }

    public IConfiguration<TTargetKey, TTargetValue> GetSubSection(TTargetKey subsectionName)
    {
        if (subsectionName is string sourceSubSectionName &&
            _source.TryGetSubSection(sourceSubSectionName, out var sourceSubSection))
        {
            return new ConfigurationProxy<TTargetKey, TTargetValue>(sourceSubSection);
        }

        return null;
    }

    public bool TryGetSubSection(TTargetKey subsectionName, out IConfiguration<TTargetKey, TTargetValue> subsection)
    {
        subsection = GetSubSection(subsectionName);
        return subsection != null;
    }

    public IEnumerable<TTargetKey> GetAllSubSectionNames()
    {
        foreach (var subsectionName in _source.GetAllSubSectionNames())
        {
            if (subsectionName is TTargetKey targetSubSectionName)
            {
                yield return targetSubSectionName;
            }
        }
    }
}