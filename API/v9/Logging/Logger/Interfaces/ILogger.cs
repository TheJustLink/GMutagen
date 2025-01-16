namespace GMutagen.v9.Logging.Logger.Interfaces
{
    public interface ILogger<T>
    {
        void Log(string message, LogLevel level = LogLevel.Info);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogDebug(string message);
    }
}