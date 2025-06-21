using System.Reflection;

namespace Serialization.Common;

public static class ReflectionPathUtils
{
    public static IEnumerable<string> GetAllPropertyPaths(this Type type, string separator = ".")
    {
        return GetPaths(type, separator, onlyFields: false, onlyProperties: true);
    }

    public static IEnumerable<string> GetAllFieldPaths(this Type type, string separator = ".")
    {
        return GetPaths(type, separator, onlyFields: true, onlyProperties: false);
    }

    public static IEnumerable<string> GetAllMemberPaths(this Type type, string separator = ".")
    {
        return GetPaths(type, separator, onlyFields: false, onlyProperties: false);
    }

    private static IEnumerable<string> GetPaths(
        Type type,
        string separator,
        bool onlyFields,
        bool onlyProperties,
        string prefix = null,
        HashSet<Type> visited = null)
    {
        if (visited == null)
            visited = new HashSet<Type>();

        if (type == typeof(string) 
            || type.IsPrimitive 
            || type.IsEnum 
            || type == typeof(List<>) 
            || type == typeof(Array)
            || type == typeof(Dictionary<,>))
            yield break;

        if (visited.Contains(type))
            yield break;

        visited.Add(type);

        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        if (!onlyProperties)
        {
            foreach (var field in type.GetFields(bindingFlags))
            {
                var memberType = field.FieldType;
                var path = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}{separator}{field.Name}";
                
                yield return path;

                foreach (var sub in GetPaths(memberType, separator, onlyFields, onlyProperties, path, visited))
                    yield return sub;
            }
        }

        if (!onlyFields)
        {
            foreach (var prop in type.GetProperties(bindingFlags))
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                    continue;

                var memberType = prop.PropertyType;
                var path = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}{separator}{prop.Name}";

                if (IsLeafType(memberType))
                {
                    yield return path;
                }
                else
                {
                    foreach (var sub in GetPaths(memberType, separator, onlyFields, onlyProperties, path, visited))
                        yield return sub;
                }
            }
        }

        visited.Remove(type);
    }

    private static bool IsLeafType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type.IsValueType;
    }
}