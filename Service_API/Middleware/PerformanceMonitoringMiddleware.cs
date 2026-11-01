using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Service_API.Middleware;

public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly long _slowThresholdMs;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _slowThresholdMs = configuration.GetValue<long>("Performance:SlowRequestThresholdMs", 1000);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        // Attach correlation ID directly to response headers without expensive OnStarting callbacks
        if (!context.Response.Headers.ContainsKey("X-Correlation-ID"))
        {
            context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var elapsedMs = (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
                int queryCount = 0;
                if (context.Items.TryGetValue(QueryCounterInterceptor.QueryCountKey, out var qVal))
                {
                    queryCount = qVal switch
                    {
                        QueryCounter qc => qc.Value,
                        int count => count,
                        _ => 0
                    };
                }

                _logger.LogDebug(
                    "{Method} {Path} {StatusCode} in {Elapsed}ms [Queries: {Queries}, TraceId: {TraceId}]",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs,
                    queryCount,
                    context.TraceIdentifier);
            }
        }
    }
}
