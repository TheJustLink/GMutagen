using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Configuration;
using Configuration.Common;
using Configuration.Providers.Interfaces;
using Configuration.Sections.Interfaces;
using Newtonsoft.Json;
using Serialization.Attributes;
using Serialization.Binders;
using Serialization.Common;
using Serialization.Models;
using Serialization.Providers;
using Serialization.Resolvers;
using Attributes_SerializableAttribute = Serialization.Attributes.SerializableAttribute;

namespace Serialization.System;

public class SerializationSystem : IDisposable
{
    private const string TEMPLATE_IDS_FILE = "template_ids.json";

    private readonly string _configPath;
    private readonly ConfigurationManager _configurationManager;
    private readonly ConcurrentDictionary<string, string> _existingIds = new();
    private readonly ConcurrentDictionary<string, List<string>> _typeIdMap = new();
    private readonly ConcurrentDictionary<string, string> _templateIds = new();
    private readonly JsonSerializerSettings _serializer;
    private readonly SerializationSystemConfigurationBinder _serializationSystemConfigurationBinder;
    private bool _disposed;

    public SerializationSystem(ConfigurationManager configurationManager, string configPath = "Configs")
    {
        _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        _configPath = Path.Combine(ProjectUtils.FindProjectRoot(Directory.GetCurrentDirectory()), configPath);
        _serializer = CreateSerializerSettings();

        _serializationSystemConfigurationBinder = new SerializationSystemConfigurationBinder(this);
        Directory.CreateDirectory(_configPath);

        LoadExistingIds();
        LoadTemplateIds();
        LoadExistingConfigurations();
    }

    public string ConfigPath => _configPath;
    public IConfiguration<string, string?>? Configuration => _configurationManager.Configuration;
    public ConfigurationManager ConfigurationManager => _configurationManager;

    #region Configuration Loading

    private void LoadExistingConfigurations()
    {
        var configFiles = GetAllConfigurationFiles().ToList();
        foreach (var file in configFiles)
        {
            var typeName = ExtractTypeName(file);
            var configId = ExtractIdFromFile(file);

            if (!string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(configId))
            {
                EnsureProviderExists(file, typeName, configId);
            }
        }
    }

    private void EnsureProviderExists(string filePath, string? typeName, string? configId)
    {
        var providerName = SerializationStringConfigurationProvider.CreateProviderName(typeName, configId);

        if (_configurationManager.GetProvider(providerName) != null)
            return;

        try
        {
            var provider = new SerializationStringConfigurationProvider(filePath,
                typeName, configId, _configurationManager.Configuration);
            _configurationManager.AddProvider(provider);

            if (!_typeIdMap.TryAdd(typeName, new List<string>() { configId }))
                _typeIdMap[typeName].Add(configId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating provider for {filePath}: {ex.Message}");
        }
    }

    private string ExtractTypeName(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directory)) return "Unknown";

        var parts = directory.Split(Path.DirectorySeparatorChar);
        return parts.LastOrDefault(p => !string.IsNullOrEmpty(p) && p != "Template") ?? "Unknown";
    }

    #endregion

    #region Configuration Access

    public object GetConfiguration(Type targetType, string? configId = null)
    {
        var typeName = targetType.Name;

        if (configId == null)
            configId = GetConfigId(targetType);

        if (configId == null)
            return Activator.CreateInstance(targetType);

        var sectionPath = ConfigurationPathConstants.GetPath(typeName, configId);
        return _configurationManager.Bind(targetType, _serializationSystemConfigurationBinder, sectionPath);
    }

    public T GetConfiguration<T>(string? configId = null) where T : class, new()
    {
        var typeName = typeof(T).Name;

        if (configId == null)
            configId = GetConfigId<T>();

        if (configId == null)
            return Activator.CreateInstance<T>();

        var sectionPath = ConfigurationPathConstants.GetPath(typeName, configId);
        return _configurationManager.Bind<T>(_serializationSystemConfigurationBinder, sectionPath);
    }

    private string? GetConfigId<T>() where T : class, new()
        => GetConfigId(typeof(T));

    private string? GetConfigId(Type targetType)
    {
        var typeName = targetType.Name;
        var id = _typeIdMap[typeName].FirstOrDefault();
        return id;
    }

    public IEnumerable<T> GetAllConfigurations<T>() where T : class, new()
    {
        var typeName = typeof(T).Name;
        var configurations = new List<T>();

        if (_configurationManager.TryGetSection(typeName, out var typeSection))
        {
            foreach (var configSectionName in typeSection.GetAllSubSectionNames())
            {
                try
                {
                    var config = _configurationManager.Bind<T>(_serializationSystemConfigurationBinder,
                        ConfigurationPathConstants.GetPath(typeName, configSectionName));
                    configurations.Add(config);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error binding configuration {typeName}:{configSectionName}: {ex.Message}");
                }
            }
        }

        return configurations;
    }

    public T GetValue<T>(string? path, T defaultValue = default)
    {
        return _configurationManager.GetValue(path, defaultValue);
    }
    
    #endregion

    #region Configuration Management

    public string CreateConfiguration<T>(T configurationObject, string customFileName = null) where T : class
    {
        var typeName = typeof(T).Name;
        var typeFolder = Path.Combine(_configPath, typeName);
        Directory.CreateDirectory(typeFolder);

        var configId = GenerateUniqueId();
        var fileName = customFileName ?? $"{typeName}_{GetNextConfigNumber(typeFolder)}.json";
        var filePath = Path.Combine(typeFolder, fileName);

        var wrapper = CreateConfigurationWrapper(configurationObject, configId);
        var json = JsonConvert.SerializeObject(wrapper, _serializer.Formatting, _serializer);
        File.WriteAllText(filePath, json);

        _existingIds.TryAdd(configId, filePath);

        EnsureProviderExists(filePath, typeName, configId);

        return configId;
    }

    public bool RemoveConfiguration(string? typeName, string? configId)
    {
        var providerName = SerializationStringConfigurationProvider.CreateProviderName(typeName, configId);

        if (_configurationManager.RemoveProvider(providerName))
        {
            var filePath = FindConfigurationFile(typeName, configId);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    _existingIds.TryRemove(configId, out _);
                    if (_typeIdMap.TryGetValue(typeName, out var list))
                        list.Remove(configId);

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting configuration file {filePath}: {ex.Message}");
                }
            }
        }

        return false;
    }

    public bool RemoveConfiguration(string? configId)
    {
        if (!_existingIds.TryGetValue(configId, out var filePath))
            return false;

        var typeName = ExtractTypeName(filePath);
        return RemoveConfiguration(typeName, configId);
    }

    #endregion

    #region Provider Management

    public void ReloadConfiguration(string? typeName, string? configId)
    {
        var providerName = SerializationStringConfigurationProvider.CreateProviderName(typeName, configId);
        _configurationManager.ReloadProvider(providerName);
    }

    public IConfigurationProvider GetProvider(string? typeName, string? configId)
    {
        var providerName = SerializationStringConfigurationProvider.CreateProviderName(typeName, configId);
        return _configurationManager.GetProvider(providerName);
    }

    public IEnumerable<IConfigurationProvider> GetAllSerializationProviders()
    {
        return _configurationManager.GetAllProviders()
            .Where(p => p.Name.StartsWith(SerializationStringConfigurationProvider.NAME_PREFIX));
    }

    #endregion

    #region Template Generation

    public void GenerateTemplates()
    {
        var serializableTypes = GetSerializableTypes();
        foreach (var type in serializableTypes)
            GenerateTemplate(type);
    }

    private IEnumerable<Type> GetSerializableTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetTypesFromAssembly)
            .Where(type => type.GetCustomAttribute<Attributes_SerializableAttribute>() != null);
    }

    private Type[] GetTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch
        {
            return new Type[0];
        }
    }

    private void GenerateTemplate(Type type)
    {
        var templatePath = GetTemplatePath(type);
        Directory.CreateDirectory(Path.GetDirectoryName(templatePath));

        var existingMetaData = ReadExistingMetaData(templatePath);
        var defaultInstance = CreateDefaultInstance(type);
        var templateId = GetOrCreateTemplateId(type.Name);

        var wrapper = new ConfigurationWrapper<object>
        {
            MetaData = new ConfigurationMetaData
            {
                Id = templateId,
            },
            Configuration = defaultInstance
        };

        var json = JsonConvert.SerializeObject(wrapper, _serializer.Formatting, _serializer);
        File.WriteAllText(templatePath, json);
    }

    #endregion

    #region Helper Methods

    private JsonSerializerSettings CreateSerializerSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Culture = CultureInfo.InvariantCulture,
            ContractResolver = new SerializeAttributeContractResolver(),
        };
    }

    private void LoadExistingIds()
    {
        if (!Directory.Exists(_configPath))
            return;

        var configFiles = GetAllConfigurationFiles();
        foreach (var file in configFiles)
        {
            var id = ExtractIdFromFile(file);
            if (!string.IsNullOrEmpty(id))
                _existingIds.TryAdd(id, file);
        }
    }

    private void LoadTemplateIds()
    {
        var templateIdsFile = Path.Combine(_configPath, TEMPLATE_IDS_FILE);
        if (!File.Exists(templateIdsFile))
            return;

        try
        {
            var json = File.ReadAllText(templateIdsFile);
            Dictionary<string, string>? templateIds = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (templateIds != null)
            {
                foreach (var kvp in templateIds)
                    _templateIds.TryAdd(kvp.Key, kvp.Value);
            }
        }
        catch
        {
            //TODO: Log error
        }
    }

    private void SaveTemplateIds()
    {
        var templateIdsFile = Path.Combine(_configPath, TEMPLATE_IDS_FILE);

        try
        {
            var templateIdsDict = _templateIds.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var json = JsonConvert.SerializeObject(templateIdsDict, Formatting.Indented);
            File.WriteAllText(templateIdsFile, json);
        }
        catch
        {
            //TODO: Log error
        }
    }

    private string? GetOrCreateTemplateId(string? typeName)
    {
        if (_templateIds.TryGetValue(typeName, out var existingId))
            return existingId;

        var newId = GenerateUniqueId();
        _templateIds.TryAdd(typeName, newId);
        SaveTemplateIds();
        return newId;
    }

    public string? ExtractIdFromFile(string file)
    {
        try
        {
            var json = File.ReadAllText(file);
            var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty(nameof(ConfigurationWrapper<object>.MetaData), out var metaData) &&
                metaData.TryGetProperty(nameof(ConfigurationWrapper<object>.MetaData.Id), out var idElement))
                return idElement.GetString();
        }
        catch
        {
            //TODO: Log error
        }

        return null;
    }

    public IEnumerable<string> GetAllConfigurationFiles()
    {
        if (!Directory.Exists(_configPath))
            return Enumerable.Empty<string>();

        var typeFolders = Directory.GetDirectories(_configPath);
        return typeFolders.SelectMany(GetConfigFilesFromTypeFolder);
    }

    private IEnumerable<string> GetConfigFilesFromTypeFolder(string typeFolder)
    {
        return Directory.GetFiles(typeFolder, "*.json", SearchOption.AllDirectories)
            .Where(f => !IsTemplateFile(f));
    }

    private bool IsTemplateFile(string filePath)
    {
        return filePath.Contains(Path.Combine("Template", "")) ||
               Path.GetFileName(filePath).EndsWith("Template.json");
    }

    private string? FindConfigurationFile(string? typeName, string? configId)
    {
        if (_existingIds.TryGetValue(configId, out var filePath))
            return filePath;

        var typeFolder = Path.Combine(_configPath, typeName);
        if (!Directory.Exists(typeFolder))
            return null;

        var configFiles = GetConfigFilesFromTypeFolder(typeFolder);
        foreach (var file in configFiles)
        {
            var id = ExtractIdFromFile(file);
            if (id == configId)
                return file;
        }

        return null;
    }

    public string? GenerateUniqueId()
    {
        string? id;
        int attempt = 0;

        do
        {
            var baseGuid = Guid.NewGuid().ToString();
            id = attempt == 0 ? baseGuid : $"{baseGuid}-{attempt}";
            attempt++;
        } while (_existingIds.ContainsKey(id) || _templateIds.ContainsKey(id));

        return id;
    }

    private ConfigurationWrapper<T> CreateConfigurationWrapper<T>(T configurationObject, string? configId)
        where T : class
    {
        return new ConfigurationWrapper<T>
        {
            MetaData = new ConfigurationMetaData
            {
                Id = configId,
            },
            Configuration = configurationObject
        };
    }

    private int GetNextConfigNumber(string typeFolder)
    {
        var configFiles = GetConfigFilesFromTypeFolder(typeFolder);
        var maxNumber = 0;

        foreach (var file in configFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var parts = fileName.Split('_');

            if (parts.Length > 1 && int.TryParse(parts[^1], out var number))
            {
                maxNumber = Math.Max(maxNumber, number);
            }
        }

        return maxNumber + 1;
    }

    private string GetTemplatePath(Type type)
    {
        return Path.Combine(_configPath, type.Name, "Template", $"{type.Name}Template.json");
    }

    private ConfigurationMetaData? ReadExistingMetaData(string templatePath)
    {
        if (!File.Exists(templatePath)) return null;

        try
        {
            var existingJson = File.ReadAllText(templatePath);
            var existingDocument = JsonDocument.Parse(existingJson);

            if (existingDocument.RootElement.TryGetProperty(nameof(ConfigurationWrapper<object>.MetaData),
                    out var metaDataElement))
                return JsonConvert.DeserializeObject<ConfigurationMetaData>(metaDataElement.GetRawText());
        }
        catch
        {
            //TODO: Log error
        }

        return null;
    }

    private object CreateDefaultInstance(Type type)
    {
        var instance = Activator.CreateInstance(type);
        if (instance == null) return new object();

        ProcessFields(type, instance);
        ProcessProperties(type, instance);
        return instance;
    }

    private void ProcessFields(Type type, object instance)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<SerializeAttribute>() != null);

        foreach (var field in fields)
        {
            var value = GetDefaultValue(field.FieldType, field.GetValue(instance));
            field.SetValue(instance, value);
        }
    }

    private void ProcessProperties(Type type, object instance)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SerializeAttribute>() != null && p.CanWrite);

        foreach (var property in properties)
        {
            var value = GetDefaultValue(property.PropertyType, property.GetValue(instance));
            property.SetValue(instance, value);
        }
    }

    private object? GetDefaultValue(Type type, object? value)
    {
        if (value != null && type.IsPrimitive)
            return value;

        if (type.IsValueType)
            return Activator.CreateInstance(type);

        if (type == typeof(string))
            return string.Empty;

        if (type.IsArray)
            return CreateDefaultArray(type);

        if (IsGenericList(type))
            return CreateDefaultList(type);

        if (type.GetCustomAttribute<Attributes_SerializableAttribute>() != null)
            return CreateDefaultInstance(type);

        return null;
    }

    private object CreateDefaultArray(Type type)
    {
        var elementType = type.GetElementType();
        var array = Array.CreateInstance(elementType, 1);
        var defaultElement = GetDefaultValue(elementType, null);
        array.SetValue(defaultElement, 0);
        return array;
    }

    private object? CreateDefaultList(Type type)
    {
        var elementType = type.GetGenericArguments()[0];
        var list = Activator.CreateInstance(type);
        var addMethod = type.GetMethod("Add");
        var defaultElement = GetDefaultValue(elementType, null);
        addMethod?.Invoke(list, new[] { defaultElement });
        return list;
    }

    private bool IsGenericList(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    #endregion

    #region Validation and Metadata

    public void ValidateAndUpdateMetaData()
    {
        var configFiles = GetAllConfigurationFiles();
        foreach (var file in configFiles)
            ValidateAndUpdateFileMetaData(file);
    }

    private void ValidateAndUpdateFileMetaData(string file)
    {
        try
        {
            var metaData = ReadMetaDataFromFile(file);

            if (ShouldUpdateMetaData(metaData))
            {
                metaData = CreateNewMetaData();
                UpdateFileMetaData(file, metaData);
                _existingIds.TryAdd(metaData.Id, file);
            }
        }
        catch
        {
            //TODO: Log error
        }
    }

    private ConfigurationMetaData ReadMetaDataFromFile(string file)
    {
        var json = File.ReadAllText(file);
        var document = JsonDocument.Parse(json);
        var metaData = new ConfigurationMetaData();

        if (document.RootElement.TryGetProperty(nameof(ConfigurationWrapper<object>.MetaData), out var metaDataElement))
        {
            if (metaDataElement.TryGetProperty(nameof(ConfigurationWrapper<object>.MetaData.Id), out var idElement))
                metaData.Id = idElement.GetString() ?? string.Empty;
        }

        return metaData;
    }

    private bool ShouldUpdateMetaData(ConfigurationMetaData metaData)
    {
        return string.IsNullOrEmpty(metaData.Id);
    }

    private ConfigurationMetaData CreateNewMetaData()
    {
        return new ConfigurationMetaData
        {
            Id = GenerateUniqueId(),
        };
    }

    private void UpdateFileMetaData(string filePath, ConfigurationMetaData metaData)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var document = JsonDocument.Parse(json);
            var configElement = document.RootElement.GetProperty(nameof(ConfigurationWrapper<object>.Configuration));

            var wrapper = new ConfigurationWrapper<object>
            {
                MetaData = metaData,
                Configuration = JsonConvert.DeserializeObject(configElement.GetRawText())
            };

            var updatedJson = JsonConvert.SerializeObject(wrapper, _serializer.Formatting, _serializer);
            File.WriteAllText(filePath, updatedJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating metadata for file {filePath}: {ex.Message}");
        }
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    #endregion

    public IEnumerable<string?> GetAllConfigurationIds(string typeName)
    {
        return _existingIds.Keys.Where(k => k.StartsWith(typeName));
    }

    public IEnumerable<string> GetAllTypeNames()
    {
        return GetSerializableTypes().Select(t => t.Name);
    }
}