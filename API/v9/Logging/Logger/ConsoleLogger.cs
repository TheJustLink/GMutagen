using System;
using System.Collections.Generic;
using GMutagen.v9.Logging.Logger.Interfaces;
using GMutagen.v9.Logging.Messages;

namespace GMutagen.v9.Logging.Logger;

public class ConsoleLogger<T> : ILogger<T>
{
    private readonly InterpolationString _interpolationString;

    public ConsoleLogger(InterpolationString interpolationString)
    {
        _interpolationString = interpolationString;
    }
    
    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        Console.ForegroundColor = GetColorForLevel(level);

        var place = new Dictionary<string, string>();
        place[$"{{{nameof(PlaceHolders.Message)}}}"] = message;
        place[$"{{{nameof(PlaceHolders.Level)}}}"] = level.ToString();

        var formattedMessage = _interpolationString.Interpolate(place);

        Console.WriteLine(formattedMessage);
        Console.ResetColor();
    }

    public void LogInfo(string message)
    {
        Log(message, LogLevel.Info);
    }

    public void LogWarning(string message)
    {
        Log(message, LogLevel.Warning);
    }

    public void LogError(string message)
    {
        Log(message, LogLevel.Error);
    }

    public void LogDebug(string message)
    {
        Log(message, LogLevel.Debug);
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