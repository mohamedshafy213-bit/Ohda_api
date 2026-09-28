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
        _slowThresholdMs = configuration.GetValue<long>("Performance:SlowRequestThresholdMs", 2000);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();
        var correlationId = context.TraceIdentifier;

        // Ensure correlation id and query count headers are present in response
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("X-Correlation-ID"))
            {
                context.Response.Headers["X-Correlation-ID"] = correlationId;
            }

            int qCount = context.Items.TryGetValue(QueryCounterInterceptor.QueryCountKey, out var val) && val is int count ? count : 0;
            if (!context.Response.Headers.ContainsKey("X-Query-Count"))
            {
                context.Response.Headers["X-Query-Count"] = qCount.ToString();
            }
            return Task.CompletedTask;
        });

        bool isLogin = context.Request.Path.Equals("/api/Auth/login", StringComparison.OrdinalIgnoreCase) &&
                       context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var endTime = DateTime.UtcNow;
            var elapsedMs = sw.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;
            int queryCount = context.Items.TryGetValue(QueryCounterInterceptor.QueryCountKey, out var qVal) && qVal is int qTotal ? qTotal : 0;

            if (isLogin)
            {
                // Dedicated single summary for the entire login flow as requested
                _logger.LogInformation(
                    "Login flow: started at {StartTime:HH:mm:ss.fff}, finished at {EndTime:HH:mm:ss.fff}, total {Elapsed}ms [Queries: {Queries}, TraceId: {TraceId}, Status: {StatusCode}]",
                    startTime, endTime, elapsedMs, queryCount, correlationId, statusCode);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Request failed: {Method} {Path} returned {StatusCode} in {Elapsed}ms [Queries: {Queries}, TraceId: {TraceId}]",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs, queryCount, correlationId);
            }
            else if (elapsedMs >= _slowThresholdMs)
            {
                _logger.LogWarning(
                    "[SLOW REQUEST] {Method} {Path} returned {StatusCode} in {Elapsed}ms [Queries: {Queries}, TraceId: {TraceId}] (exceeded threshold {Threshold}ms)",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs, queryCount, correlationId, _slowThresholdMs);
            }
            else
            {
                _logger.LogInformation(
                    "{Method} {Path} {StatusCode} in {Elapsed}ms [Queries: {Queries}, TraceId: {TraceId}]",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs, queryCount, correlationId);
            }
        }
    }
}
