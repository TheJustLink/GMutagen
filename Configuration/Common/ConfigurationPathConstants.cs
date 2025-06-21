namespace Configuration.Common;

//TODO: Replace with configuration for more fluid behavior
public abstract class ConfigurationPathConstants
{
    public const string PATH_DELIMITER = ":";
    
    public static string GetPath(string? typeName, string? configurationId)
    {
        return $"{typeName}{PATH_DELIMITER}{configurationId}";
    }
}