using Serilog;
using Serilog.Sinks.Database;
using static ASql.ASqlManager;

namespace LoggerService
{
    public class LoggerManager : ILoggerManager
    {
        private static ILogger logger;

        public LoggerManager(string dataBaseProvider, string dataBaseConnectionString)
        {
            try
            {
                var provider = (dataBaseProvider ?? string.Empty).Trim();
                var conn = (dataBaseConnectionString ?? string.Empty).Trim();

                if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(conn))
                {
                    logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.Console()
                        .WriteTo.Database(DBType.Oracle, conn, "SerLogs", Serilog.Events.LogEventLevel.Warning, false, 1)
                        .CreateLogger();
                    return;
                }

                if ((provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) ||
                     provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(conn))
                {
                    logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.Console()
                        .WriteTo.Database(DBType.PostgreSQL, conn, "SerLogs", Serilog.Events.LogEventLevel.Warning, false, 1)
                        .CreateLogger();
                    return;
                }

                // SqlServer and default fallback: clean, high-performance console logging
                logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .CreateLogger();
            }
            catch (Exception ex)
            {
                logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .CreateLogger();
                logger.Error(ex, "Failed to initialize logger; using console fallback.");
            }
        }

        
        public void LogDebug(string message) => logger.Debug(message);

        public void LogError(string message) => logger.Error(message);

        public void LogInfo(string message) => logger.Information(message);

        public void LogWarn(string message) => logger.Warning(message);



        public void logDebugWithException(Exception exception, string message)
        {
            logger.Debug(exception, message);
        }

        public void logErrorWithException(Exception exception, string message)
        {
            logger.Error(exception, message);
        }

    }
}
