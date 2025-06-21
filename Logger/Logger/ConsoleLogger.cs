using System.Runtime.CompilerServices;
using Logger.Common;
using Logger.Extensions;
using Logger.Logger.Interfaces;

namespace Logger.Logger;

public class ConsoleLogger<T>(InterpolationString interpolationString) : ILogger<T>
{
    public ConsoleLogger() : this(Constants.LoggerPattern)
    {
    }

    public ConsoleLogger(string pattern) : this(new InterpolationString(pattern))
    {
    }

    public void Log(string message, LogLevel level = LogLevel.Info,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Console.ForegroundColor = GetColorForLevel(level);

        var place = new Dictionary<string, string>();
        place[$"{{{nameof(PlaceHolders.Message)}}}"] = message;
        place[$"{{{nameof(PlaceHolders.Level)}}}"] = level.ToString();

        place.AddDebugInfo(filePath, lineNumber, memberName);

        var formattedMessage = interpolationString.Interpolate(place);

        Console.WriteLine(formattedMessage);
        Console.ResetColor();
    }

    public void LogInfo(string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Log(message, LogLevel.Info, filePath, lineNumber, memberName);
    }

    public void LogWarning(string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Log(message, LogLevel.Warning, filePath, lineNumber, memberName);
    }

    public void LogError(string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Log(message, LogLevel.Error, filePath, lineNumber, memberName);
    }

    public void LogDebug(string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        Log(message, LogLevel.Debug, filePath, lineNumber, memberName);
    }

    private ConsoleColor GetColorForLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Debug => ConsoleColor.Cyan,
            _ => ConsoleColor.White,
        };
    }
}

public class Constants
{
    public static string LoggerPattern { get; } = $"[{{{nameof(PlaceHolders.Date)}}} " +
                                                  $"{{{nameof(PlaceHolders.Time)}}}] " +
                                                  $"[{{{nameof(PlaceHolders.Level)}}}] " +
                                                  $"{{{nameof(PlaceHolders.Message)}}}";
}