using System.Reflection;
using System.Text.Json;
using Configuration.Binders;
using Configuration.Sections.Interfaces;
using Serialization.Attributes;
using Serialization.System;

namespace Serialization.Binders;

public class SerializationSystemConfigurationBinder : IBinder<string, string>
{
    private readonly SerializationSystem _serializationSystem;
    
    public SerializationSystemConfigurationBinder(SerializationSystem serializationSystem)
    {
        _serializationSystem = serializationSystem;
    }
    
    public void Bind(object instance, Type type, IConfiguration<string, string?> section)
    {
        if (instance == null || section == null) return;
        
        BindFields(instance, type, section);
        BindProperties(instance, type, section);
    }
    
    private void BindFields(object instance, Type type, IConfiguration<string, string?> section)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<SerializeAttribute>() != null);
        
        foreach (var field in fields)
        {
            BindMember(instance, field.Name, field.FieldType, section, 
                value => field.SetValue(instance, value));
        }
    }
    
    private void BindProperties(object instance, Type type, IConfiguration<string, string?> section)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SerializeAttribute>() != null && p.CanWrite);
        
        foreach (var property in properties)
        {
            BindMember(instance, property.Name, property.PropertyType, section, 
                value => property.SetValue(instance, value));
        }
    }
    
    private void BindMember(object instance, string memberName, Type memberType, 
        IConfiguration<string, string?> section, Action<object?> setValue)
    {
        if (!section.TryGetValue(memberName, out var configValue) || configValue == null) 
            return;
        
        try
        {
            var boundValue = BindValue(configValue, memberType, section, memberName);
            setValue(boundValue);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error binding member '{memberName}' of type '{memberType.Name}': {ex.Message}");
        }
    }
    
    private object? BindValue(string configValue, Type targetType, IConfiguration<string, string?> parentSection, string basePath)
    {
        if (string.IsNullOrEmpty(configValue)) 
            return GetDefaultValue(targetType);
        
        if (targetType == typeof(string))
        {
            return configValue;
        }
        
        if (IsSimpleType(targetType))
        {
            return ConvertSimpleType(configValue, targetType);
        }
        
        if (targetType.IsArray)
        {
            return BindArray(configValue, targetType, parentSection, basePath);
        }
        
        if (IsGenericList(targetType))
        {
            return BindList(configValue, targetType, parentSection, basePath);
        }
        
        if (IsComplexType(targetType))
        {
            return BindComplexType(configValue, targetType, parentSection, basePath);
        }
        
        return configValue;
    }
    
    private object? BindComplexType(string configValue, Type targetType, IConfiguration<string, string?> parentSection, string basePath)
    {
        if (IsGuidString(configValue))
        {
            return LoadObjectByConfigId(configValue, targetType);
        }
        
        try
        {
            var jsonDoc = JsonDocument.Parse(configValue);
            return BindComplexTypeFromJson(jsonDoc.RootElement, targetType);
        }
        catch
        {
            return BindComplexTypeFromSection(configValue, targetType, parentSection, basePath);
        }
    }
    
    private object? BindComplexTypeFromJson(JsonElement jsonElement, Type targetType)
    {
        var instance = Activator.CreateInstance(targetType);
        if (instance == null) return null;
        
        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<SerializeAttribute>() != null);
        
        var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SerializeAttribute>() != null && p.CanWrite);
        
        foreach (var field in fields)
        {
            if (jsonElement.TryGetProperty(field.Name, out var propValue))
            {
                var value = ConvertJsonElementToValue(propValue, field.FieldType);
                field.SetValue(instance, value);
            }
        }
        
        foreach (var property in properties)
        {
            if (jsonElement.TryGetProperty(property.Name, out var propValue))
            {
                var value = ConvertJsonElementToValue(propValue, property.PropertyType);
                property.SetValue(instance, value);
            }
        }
        
        return instance;
    }
    
    private object? BindComplexTypeFromSection(string configValue, Type targetType, IConfiguration<string, string?> parentSection, string basePath)
    {
        var instance = Activator.CreateInstance(targetType);
        if (instance == null) return null;
        
        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<SerializeAttribute>() != null);
        
        var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SerializeAttribute>() != null && p.CanWrite);
        
        foreach (var field in fields)
        {
            var fieldPath = $"{basePath}:{field.Name}";
            if (parentSection.TryGetValue(fieldPath, out var fieldValue) && fieldValue != null)
            {
                var value = BindValue(fieldValue, field.FieldType, parentSection, fieldPath);
                field.SetValue(instance, value);
            }
        }
        
        foreach (var property in properties)
        {
            var propertyPath = $"{basePath}:{property.Name}";
            if (parentSection.TryGetValue(propertyPath, out var propertyValue) && propertyValue != null)
            {
                var value = BindValue(propertyValue, property.PropertyType, parentSection, propertyPath);
                property.SetValue(instance, value);
            }
        }
        
        return instance;
    }
    
    private object? BindArray(string configValue, Type arrayType, IConfiguration<string, string?> parentSection, string basePath)
    {
        var elementType = arrayType.GetElementType();
        if (elementType == null) return null;
        
        try
        {
            var jsonDoc = JsonDocument.Parse(configValue);
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var items = new List<object?>();
                
                foreach (var element in jsonDoc.RootElement.EnumerateArray())
                {
                    var itemValue = ConvertJsonElementToValue(element, elementType);
                    items.Add(itemValue);
                }
                
                var array = Array.CreateInstance(elementType, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    array.SetValue(items[i], i);
                }
                
                return array;
            }
        }
        catch
        {
            return BindArrayFromSection(elementType, parentSection, basePath);
        }
        
        return null;
    }
    
    private object? BindArrayFromSection(Type elementType, IConfiguration<string, string?> parentSection, string basePath)
    {
        var items = new List<object?>();
        int index = 0;
        
        while (true)
        {
            var itemPath = $"{basePath}:{index}";
            if (parentSection.TryGetValue(itemPath, out var itemValue) && itemValue != null)
            {
                var boundItem = BindValue(itemValue, elementType, parentSection, itemPath);
                items.Add(boundItem);
                index++;
            }
            else
            {
                break;
            }
        }
        
        if (items.Count == 0) return null;
        
        var array = Array.CreateInstance(elementType, items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            array.SetValue(items[i], i);
        }
        
        return array;
    }
    
    private object? BindList(string configValue, Type listType, IConfiguration<string, string?> parentSection, string basePath)
    {
        var elementType = listType.GetGenericArguments()[0];
        var list = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add");
        
        if (list == null || addMethod == null) return null;
        
        try
        {
            var jsonDoc = JsonDocument.Parse(configValue);
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in jsonDoc.RootElement.EnumerateArray())
                {
                    var itemValue = ConvertJsonElementToValue(element, elementType);
                    addMethod.Invoke(list, new[] { itemValue });
                }
                
                return list;
            }
        }
        catch
        {
            return BindListFromSection(list, elementType, addMethod, parentSection, basePath);
        }
        
        return list;
    }
    
    private object BindListFromSection(object list, Type elementType, MethodInfo addMethod, 
        IConfiguration<string, string?> parentSection, string basePath)
    {
        int index = 0;
        
        while (true)
        {
            var itemPath = $"{basePath}:{index}";
            if (parentSection.TryGetValue(itemPath, out var itemValue) && itemValue != null)
            {
                var boundItem = BindValue(itemValue, elementType, parentSection, itemPath);
                addMethod.Invoke(list, new[] { boundItem });
                index++;
            }
            else
            {
                break;
            }
        }
        
        return list;
    }
    
    private object? ConvertJsonElementToValue(JsonElement element, Type targetType)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var stringValue = element.GetString();
                if (IsComplexType(targetType) && IsGuidString(stringValue))
                {
                    return LoadObjectByConfigId(stringValue, targetType);
                }
                return BindValue(stringValue ?? string.Empty, targetType, null, string.Empty);
                
            case JsonValueKind.Number:
                return ConvertSimpleType(element.GetDecimal(), targetType);
                
            case JsonValueKind.True:
            case JsonValueKind.False:
                return ConvertSimpleType(element.GetBoolean(), targetType);
                
            case JsonValueKind.Array:
                if (targetType.IsArray)
                {
                    return BindArrayFromJsonElement(element, targetType);
                }
                if (IsGenericList(targetType))
                {
                    return BindListFromJsonElement(element, targetType);
                }
                break;
                
            case JsonValueKind.Object:
                if (IsComplexType(targetType))
                {
                    return BindComplexTypeFromJson(element, targetType);
                }
                break;
                
            case JsonValueKind.Null:
                return GetDefaultValue(targetType);
        }
        
        return GetDefaultValue(targetType);
    }
    
    private object? BindArrayFromJsonElement(JsonElement element, Type arrayType)
    {
        var elementType = arrayType.GetElementType();
        if (elementType == null) return null;
        
        var items = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            var value = ConvertJsonElementToValue(item, elementType);
            items.Add(value);
        }
        
        var array = Array.CreateInstance(elementType, items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            array.SetValue(items[i], i);
        }
        
        return array;
    }
    
    private object? BindListFromJsonElement(JsonElement element, Type listType)
    {
        var elementType = listType.GetGenericArguments()[0];
        var list = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add");
        
        if (list == null || addMethod == null) return null;
        
        foreach (var item in element.EnumerateArray())
        {
            var value = ConvertJsonElementToValue(item, elementType);
            addMethod.Invoke(list, new[] { value });
        }
        
        return list;
    }
    
    private object? LoadObjectByConfigId(string? configId, Type targetType)
    {
        if (string.IsNullOrEmpty(configId) || _serializationSystem == null)
        {
            Console.WriteLine($"Cannot resolve configId '{configId}' - Serialization not available");
            return GetDefaultValue(targetType);
        }
        
        try
        {
            return _serializationSystem.GetConfiguration(targetType, configId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading object by configId '{configId}': {ex.Message}");
            return GetDefaultValue(targetType);
        }
    }
    
    private bool IsGuidString(string? value)
    {
        return !string.IsNullOrEmpty(value) && Guid.TryParse(value, out _);
    }
    
    private bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || 
               type == typeof(decimal) || 
               type == typeof(DateTime) || 
               type == typeof(DateTimeOffset) || 
               type == typeof(TimeSpan) || 
               type == typeof(Guid) ||
               type.IsEnum ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                IsSimpleType(type.GetGenericArguments()[0]));
    }
    
    private bool IsGenericList(Type type)
    {
        return type.IsGenericType && 
               (type.GetGenericTypeDefinition() == typeof(List<>) ||
                type.GetGenericTypeDefinition() == typeof(IList<>) ||
                type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }
    
    private bool IsComplexType(Type type)
    {
        return !IsSimpleType(type) && 
               !type.IsArray && 
               !IsGenericList(type) && 
               type != typeof(string) &&
               type.IsClass;
    }
    
    private object? ConvertSimpleType(object value, Type targetType)
    {
        try
        {
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null) return null;
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                return Convert.ChangeType(value, underlyingType!);
            }
            
            if (targetType.IsEnum)
            {
                if (value is string stringValue)
                    return Enum.Parse(targetType, stringValue, true);
                return Enum.ToObject(targetType, value);
            }
            
            return Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting value '{value}' to type '{targetType.Name}': {ex.Message}");
            return GetDefaultValue(targetType);
        }
    }
    
    private object? GetDefaultValue(Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        
        return null;
    }
}