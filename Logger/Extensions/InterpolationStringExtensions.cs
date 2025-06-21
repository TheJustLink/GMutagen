using Logger.Common;
using Logger.Logger;

namespace Logger.Extensions;

public static class InterpolationStringExtensions
{
    public static Dictionary<string, string> AddDebugInfo(this Dictionary<string, string> placeholders, string filePath = "",
        int lineNumber = 0,
        string memberName = "")
    {
        placeholders.Add($"{{{nameof(PlaceHolders.FilePath)}}}", filePath.ToString());
        placeholders.Add($"{{{nameof(PlaceHolders.LineNumber)}}}", lineNumber.ToString());
        placeholders.Add($"{{{nameof(PlaceHolders.MemberName)}}}", memberName.ToString());
        return placeholders;
    }
    
    public static Dictionary<string, string> AddMessage(this Dictionary<string, string> placeholders, string message)
    {
        placeholders.Add($"{{{nameof(PlaceHolders.Message)}}}", message);
        return placeholders;
    }
    
    public static Dictionary<string, string> AddSender(this Dictionary<string, string> placeholders, object sender)
    {
        placeholders.Add($"{{{nameof(PlaceHolders.Sender)}}}", sender.ToString());
        return placeholders;
    }
    
    
    public static InterpolationString AddInfo(this InterpolationString interpolationString)
    {
        return interpolationString
            .AddPlaceholder($"{{{nameof(PlaceHolders.Time)}}}", () => DateTime.Now.ToString("HH:mm:ss"))
            .AddPlaceholder($"{{{nameof(PlaceHolders.Date)}}}", () => DateTime.Now.ToString("yyyy-MM-dd"));
    }
}