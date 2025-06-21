using System.Reflection;
using Configuration.Sections.Interfaces;

namespace Configuration.Binders;

public class DefaultConfigurationBinder : IBinder<string, object>
{
    private const BindingFlags MEMBER_BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

    public void Bind(object instance, Type type, IConfiguration<string, object> configuration)
    {
        ValidateBindingParameters(instance, type, configuration);
        
        BindProperties(instance, type, configuration);
        BindFields(instance, type, configuration);
    }

    private static void ValidateBindingParameters(object instance, Type type, IConfiguration<string, object> configuration)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(configuration);
    }

    private void BindProperties(object instance, Type type, IConfiguration<string, object> configuration)
    {
        var properties = GetProperties(type);
        foreach (var property in properties)
        {
            BindMember(instance, configuration, property.Name, property.PropertyType, property.SetValue);
        }
    }

    private void BindFields(object instance, Type type, IConfiguration<string, object> configuration)
    {
        var fields = GetFields(type);
        foreach (var field in fields)
        {
            BindMember(instance, configuration, field.Name, field.FieldType, field.SetValue);
        }
    }

    private void BindMember(object instance, IConfiguration<string, object> configuration, string memberName, Type memberType, Action<object, object?> setValue)
    {
        if (TryBindSimpleValue(configuration, memberName, memberType, out var convertedValue))
        {
            SetMemberValue(instance, setValue, convertedValue);
        }
        else if (TryBindComplexObject(configuration, memberName, memberType, out var complexObject))
        {
            SetMemberValue(instance, setValue, complexObject);
        }
    }

    private bool TryBindSimpleValue(IConfiguration<string, object> configuration, string memberName, Type memberType, out object? convertedValue)
    {
        convertedValue = null;
        
        if (!configuration.TryGetValue(memberName, out var rawValue))
            return false;

        return TryConvertValue(rawValue, memberType, out convertedValue);
    }

    private bool TryBindComplexObject(IConfiguration<string, object> configuration, string memberName, Type memberType, out object? complexObject)
    {
        complexObject = null;
        
        if (!ShouldBindAsComplexObject(memberType))
            return false;

        if (!configuration.TryGetSubSection(memberName, out var subSection))
            return false;

        return TryCreateAndBindComplexObject(memberType, subSection, out complexObject);
    }

    private bool TryCreateAndBindComplexObject(Type objectType, IConfiguration<string, object> subSection, out object? complexObject)
    {
        complexObject = null;
        
        if (!TryCreateInstance(objectType, out var instance))
            return false;

        Bind(instance, objectType, subSection);
        complexObject = instance;
        return true;
    }

    private static bool TryConvertValue(object? rawValue, Type targetType, out object? convertedValue)
    {
        convertedValue = null;
        
        if (rawValue is null)
            return false;

        try
        {
            if (rawValue.GetType() == targetType)
            {
                convertedValue = rawValue;
                return true;
            }

            convertedValue = Convert.ChangeType(rawValue, targetType);
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
        catch (ArgumentNullException)
        {
            return false;
        }
    }

    private static bool TryCreateInstance(Type type, out object? instance)
    {
        instance = null;
        
        try
        {
            instance = Activator.CreateInstance(type);
            return instance is not null;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (TargetInvocationException)
        {
            return false;
        }
        catch (MethodAccessException)
        {
            return false;
        }
        catch (MemberAccessException)
        {
            return false;
        }
    }

    private static void SetMemberValue(object instance, Action<object, object?> setValue, object? value)
    {
        try
        {
            setValue(instance, value);
        }
        catch (ArgumentException)
        {
        }
        catch (TargetException)
        {
        }
        catch (TargetInvocationException)
        {
        }
    }

    private static bool ShouldBindAsComplexObject(Type memberType)
    {
        return memberType.IsClass && 
               memberType != typeof(string) && 
               !memberType.IsPrimitive;
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        return type.GetProperties(MEMBER_BINDING_FLAGS);
    }

    private static FieldInfo[] GetFields(Type type)
    {
        return type.GetFields(MEMBER_BINDING_FLAGS);
    }
}