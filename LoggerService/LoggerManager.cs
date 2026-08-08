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
            // Always start with a non-null fallback logger to avoid NullReferenceException
            // if database sink configuration fails (bad provider, missing connection string, DB down, etc.).
            logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var provider = (dataBaseProvider ?? string.Empty).Trim();
                var conn = (dataBaseConnectionString ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(conn))
                {
                    logger.Warning("Logger database sink disabled: DatabaseProvider/connection string not set.");
                    return;
                }

                if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
                {
                    logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Database(DBType.Oracle, conn, "SerLogs", Serilog.Events.LogEventLevel.Verbose, false, 1)
                        .CreateLogger();
                    return;
                }

                if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) ||
                    provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                {
                    logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Database(DBType.PostgreSQL, conn, "SerLogs", Serilog.Events.LogEventLevel.Verbose, false, 1)
                        .CreateLogger();
                    return;
                }

                logger.Warning("Logger database sink disabled: unsupported DatabaseProvider '{Provider}'.", provider);
            }
            catch (Exception ex)
            {
                // Keep fallback logger alive; surface the failure for diagnostics.
                logger.Error(ex, "Failed to configure logger database sink; using console fallback.");
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
