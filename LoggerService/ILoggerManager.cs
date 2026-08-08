namespace LoggerService
{
    public interface ILoggerManager
    {
        void logDebugWithException(Exception exception, string message);
        void logErrorWithException(Exception exception, string message);
        void LogInfo(string message);
        void LogWarn(string message);
        void LogDebug(string message);
        void LogError(string message);
    }
}
