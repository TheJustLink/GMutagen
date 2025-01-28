using System;
using System.Collections.Generic;

namespace GMutagen.v9.Logging.Common;

public class InterpolationString
{
    private readonly string _template;
    private readonly Dictionary<string, Func<string>> _placeholders;

    public InterpolationString(string template, Dictionary<string, Func<string>> customPlaceholders = null)
    {
        _template = template;
        _placeholders = customPlaceholders ?? new Dictionary<string, Func<string>>();
    }

    public InterpolationString AddPlaceholder(string key, Func<string> value)
    {
        _placeholders[key] = value;
        return this;
    }

    public InterpolationString RemovePlaceholder(string key)
    {
        _placeholders.Remove(key);
        return this;
    }

    public string Interpolate(Dictionary<string, string> placeholders)
    {
        string result = _template;

        if (_template.Contains("{Time}"))
        {
            result = result.Replace("{Time}", DateTime.Now.ToString("HH:mm:ss"));
        }

        if (_template.Contains("{Date}"))
        {
            result = result.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        foreach (var placeholder in placeholders)
        {
            if (result.Contains(placeholder.Key))
            {
                result = result.Replace(placeholder.Key, placeholder.Value);
            }
        }
        
        foreach (var placeholder in _placeholders)
        {
            if (result.Contains(placeholder.Key))
            {
                result = result.Replace(placeholder.Key, placeholder.Value.Invoke());
            }
        }

        return result;
    }
}