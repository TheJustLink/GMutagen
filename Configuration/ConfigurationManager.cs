using System.Collections.Concurrent;
using Configuration.Binders;
using Configuration.Common;
using Configuration.Providers.Interfaces;
using Configuration.Sections.Interfaces;
using Configuration.Sections.Realizations;

namespace Configuration;

public class ConfigurationManager : IDisposable
{
    private readonly IConfiguration<string, string> _configuration;
    private readonly ConcurrentDictionary<string, IConfigurationProvider> _providers = new();
    private readonly object _lock = new();
    private readonly DefaultConfigurationBinder _configurationBinder = new();
    private bool _disposed;
    
    public ConfigurationManager(IConfiguration<string, string>? configuration = null)
    {
        _configuration = configuration ?? new FlattenedStringConfiguration<string>();
    }

    public IConfiguration<string, string> Configuration => _configuration;


    public void AddProvider(IConfigurationProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        lock (_lock)
        {
            if (!_providers.TryAdd(provider.Name, provider)) 
                return;
            
            provider.Load();
        }
    }

    public bool RemoveProvider(string providerName)
    {
        lock (_lock)
        {
            if (!_providers.TryRemove(providerName, out var provider))
                return false;
            
            provider.Dispose();
            return true;
        }

        return false;
    }

    public IConfigurationProvider GetProvider(string providerName)
    {
        lock (_lock)
        {
            _providers.TryGetValue(providerName, out var provider);
            return provider;
        }
    }

    public IEnumerable<IConfigurationProvider> GetAllProviders()
    {
        lock (_lock)
        {
            return _providers.Values.OrderBy(p => p.Priority);
        }
    }
    
    public T GetValue<T>(string? path, T defaultValue = default)
    {
        if (TryGetValue(path, out T value))
            return value;
        
        return defaultValue;
    }

    public bool TryGetValue<T>(string path, out T value)
    {
        value = default;

        if (_configuration is FlattenedStringConfiguration<string> flatConfig &&
            flatConfig.TryGetValueByPath(path, out var rawValue))
        {
            return TryConvertValue(rawValue, out value);
        }
        
        var keys = path.Split(ConfigurationPathConstants.PATH_DELIMITER, StringSplitOptions.RemoveEmptyEntries);
        var currentSection = _configuration;

        var last = keys.Length - 1;
        for (var i = 0; i < last; i++)
        {
            if (!currentSection.TryGetSubSection(keys[i], out currentSection))
                return false;
        }

        if (currentSection.TryGetValue(keys[last], out var configValue))
        {
            return TryConvertValue(configValue, out value);
        }

        return false;
    }

    private IConfiguration<string, string> CreateSubSection()
    {
        return _configuration is FlattenedStringConfiguration<string>
            ? new FlattenedStringConfiguration<string>()
            : new ConfigurationSection<string, string>();
    }

    public IConfiguration<string, string> GetSection(string sectionName)
    {
        return _configuration.GetSubSection(sectionName);
    }

    public bool TryGetSection(string sectionName, out IConfiguration<string, string>? section)
    {
        return _configuration.TryGetSubSection(sectionName, out section);
    }

    public object Bind(Type targetType, IBinder<string, string>? binder = null, string? sectionPath = null)
    {
        var section = string.IsNullOrEmpty(sectionPath) ? _configuration : GetSection(sectionPath);
        var instance = Activator.CreateInstance(targetType);
        return BindConfigurationToObject(instance, targetType, section, binder);
    }
    public T Bind<T>(T instance, IBinder<string, string>? binder = null, string? sectionPath = null) where T : class, new()
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        var section = string.IsNullOrEmpty(sectionPath) ? _configuration : GetSection(sectionPath);
        return (T)BindConfigurationToObject(instance, typeof(T), section, binder);
    }

    public T Bind<T>(IBinder<string, string>? binder = null, string? sectionPath = null) where T : class, new()
    {
        var section = string.IsNullOrEmpty(sectionPath) ? _configuration : GetSection(sectionPath);
        var instance = Activator.CreateInstance<T>();
        return (T)BindConfigurationToObject(instance, typeof(T), section, binder);
    }

    private object BindConfigurationToObject(object instance, Type type, IConfiguration<string, string> section, IBinder<string, string> binder)
    {
        binder.Bind(instance,type , section);
        return instance;
    }
    
    public void ReloadAllProviders()
    {
        lock (_lock)
        {
            foreach (var provider in _providers.Values.OrderBy(p => p.Priority))
            {
                provider.Unload();
                provider.Load();
            }
        }
    }

    public void ReloadProvider(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var provider))
        {
            provider.Unload();
            provider.Load();
        }
    }
    
    private static bool TryConvertValue<T>(object value, out T result)
    {
        result = default;

        try
        {
            if (value is T directValue)
            {
                result = directValue;
                return true;
            }

            if (value != null)
            {
                result = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
        }
        catch
        {
        }

        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            foreach (var provider in _providers.Values)
            {
                provider.Dispose();
            }

            _providers.Clear();
        }

        _disposed = true;
    }
}
