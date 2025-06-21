using System.Reflection;
using Configuration;
using Configuration.Common;
using Configuration.Sections.Interfaces;
using Configuration.Sections.Realizations;
using Microsoft.Extensions.DependencyInjection;
using Serialization.Attributes;
using Serialization.Common;
using Serialization.Providers;
using Serialization.System;

namespace Serialization.Extensions;

public static class ServiceCollectionExtensions
{
    private static SerializationSystem _serializationSystem;
    private static ConfigurationManager _configurationManager;
    private static IConfiguration<string, string> _configuration;

    public static IServiceCollection AddFlattenConfiguration(this IServiceCollection services)
        => services.AddConfiguration(ConfigurationMode.Flatten);

    public static IServiceCollection AddTreeConfiguration(this IServiceCollection services)
        => services.AddConfiguration(ConfigurationMode.Tree);

    public static IServiceCollection AddConfiguration(this IServiceCollection services,
        ConfigurationMode mode = ConfigurationMode.Flatten)
    {
        switch (mode)
        {
            case ConfigurationMode.Flatten:
                _configuration = new DefaultFlattenedConfiguration();
                break;

            case ConfigurationMode.Tree:
                _configuration = new DefaultConfigurationSection();
                break;
        }

        services.AddSingleton(_configuration);
        return services;
    }

    public static IServiceCollection AddSerializationSystem(this IServiceCollection services,
        string configPath = "Configs")
    {
        EnsureConfigurationManagerInitialized();

        _serializationSystem = new SerializationSystem(_configurationManager, configPath);
        _serializationSystem.GenerateTemplates();
        _serializationSystem.ValidateAndUpdateMetaData();

        services.AddSingleton(_serializationSystem);
        return services;
    }

    public static IServiceCollection AddConfigurationManager(this IServiceCollection services)
    {
        EnsureConfigurationInitialized();

        _configurationManager = new ConfigurationManager(_configuration);

        services.AddSingleton(_configurationManager);
        return services;
    }

    private static void EnsureConfigurationInitialized()
    {
        if (_configuration == null)
            throw new InvalidOperationException(
                $"{nameof(_configuration)} not initialized. Call {nameof(AddConfiguration)} first.");
    }

    private static void EnsureSerializationSystemInitialized()
    {
        if (_serializationSystem == null)
            throw new InvalidOperationException(
                $"{nameof(_serializationSystem)} not initialized. Call {nameof(AddSerializationSystem)} first.");
    }

    private static void EnsureConfigurationManagerInitialized()
    {
        if (_configurationManager == null)
            throw new InvalidOperationException(
                $"{nameof(_configurationManager)} not initialized. Call {nameof(AddConfigurationManager)} first.");
    }

    public static IServiceCollection RegisterConfigurable<T>(this IServiceCollection services, string configId = null)
        where T : class, new()
    {
        EnsureConfigurationManagerInitialized();

        services.AddTransient<T>(provider =>
        {
            var configManager = provider.GetRequiredService<SerializationSystem>();
            var config = configManager.GetConfiguration<T>();
            InjectDependencies(config, provider);
            return config;
        });

        return services;
    }

    public static T ResolveConfigurable<T>(this IServiceProvider serviceProvider, string? configId = null)
        where T : class, new()
    {
        var configManager = serviceProvider.GetRequiredService<SerializationSystem>();
        var config = configManager.GetConfiguration<T>(configId);
        InjectDependencies(config, serviceProvider);
        return config;
    }

    public static IConfiguration<string, string> ResolveConfiguration<T>(this IServiceProvider serviceProvider,
        string configId = null) where T : class, new()
    {
        var configManager = serviceProvider.GetRequiredService<ConfigurationManager>();
        return configManager.GetSection(typeof(T).Name).GetSubSection(configId);
    }

    public static T GetConfigurationValue<T>(this IServiceProvider serviceProvider, string typeName, string path,
        string configId = null, T defaultValue = default)
    {
        var configManager = serviceProvider.GetRequiredService<ConfigurationManager>();
        var fullPath = string.Join(ConfigurationPathConstants.PATH_DELIMITER, typeName, configId, path);
        return configManager.GetValue(fullPath, defaultValue);
    }

    public static string? CreateConfiguration<T>(this IServiceProvider serviceProvider, T configurationObject,
        string customFileName = null) where T : class
    {
        var configManager = serviceProvider.GetRequiredService<SerializationSystem>();
        return configManager.CreateConfiguration(configurationObject, customFileName);
    }

    public static void ReloadAllProviders(this IServiceProvider serviceProvider)
    {
        var configManager = serviceProvider.GetRequiredService<ConfigurationManager>();
        configManager.ReloadAllProviders();
    }

    public static void ReloadConfiguration(this IServiceProvider serviceProvider, string? typeName, string? configId)
    {
        var configManager = serviceProvider.GetRequiredService<ConfigurationManager>();
        configManager.ReloadProvider(SerializationStringConfigurationProvider.CreateProviderName(typeName, configId));
    }

    public static IEnumerable<string?> GetAllConfigurationIds(this IServiceProvider serviceProvider, string typeName)
    {
        var configManager = serviceProvider.GetRequiredService<SerializationSystem>();
        return configManager.GetAllConfigurationIds(typeName);
    }

    public static IEnumerable<string> GetAllTypeNames(this IServiceProvider serviceProvider)
    {
        var configManager = serviceProvider.GetRequiredService<SerializationSystem>();
        return configManager.GetAllTypeNames();
    }

    private static void InjectDependencies(object instance, IServiceProvider serviceProvider)
    {
        var type = instance.GetType();
        InjectIntoFields(instance, type, serviceProvider);
        InjectIntoProperties(instance, type, serviceProvider);
    }

    private static void InjectIntoFields(object instance, Type type, IServiceProvider serviceProvider)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (ShouldInjectDependency(field))
            {
                var service = serviceProvider.GetService(field.FieldType);
                if (service != null)
                    field.SetValue(instance, service);
            }
        }
    }

    private static void InjectIntoProperties(object instance, Type type, IServiceProvider serviceProvider)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (ShouldInjectDependency(property))
            {
                var service = serviceProvider.GetService(property.PropertyType);
                if (service != null)
                    property.SetValue(instance, service);
            }
        }
    }

    private static bool ShouldInjectDependency(FieldInfo field)
    {
        return field.FieldType.IsInterface ||
               (field.FieldType.IsClass && field.GetCustomAttribute<SerializeAttribute>() == null);
    }

    private static bool ShouldInjectDependency(PropertyInfo property)
    {
        return property.PropertyType.IsInterface ||
               (property.PropertyType.IsClass && property.GetCustomAttribute<SerializeAttribute>() == null);
    }
}