using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service_API.Services;

public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Task Queue Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    // Propagate captured tenant context explicitly to the scoped service provider
                    var tenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
                    tenant.SetTenant(workItem.BranchId, workItem.UserId, workItem.IsSuperAdmin);

                    await workItem.WorkItem(scope.ServiceProvider, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background work item for Branch {BranchId}.", workItem.BranchId);
                }
            }
            catch (OperationCanceledException)
            {
                // Task was canceled, exit gracefully
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background task dequeue loop.");
            }
        }

        _logger.LogInformation("Background Task Queue Worker stopped.");
    }
}
