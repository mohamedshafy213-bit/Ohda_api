using System;
using System.Threading;
using System.Threading.Tasks;

namespace Service_API.Services;

public class BackgroundWorkItem
{
    public Func<IServiceProvider, CancellationToken, ValueTask> WorkItem { get; }
    public int? BranchId { get; }
    public int? UserId { get; }
    public bool IsSuperAdmin { get; }

    public BackgroundWorkItem(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem,
        int? branchId = null,
        int? userId = null,
        bool isSuperAdmin = false)
    {
        WorkItem = workItem ?? throw new ArgumentNullException(nameof(workItem));
        BranchId = branchId;
        UserId = userId;
        IsSuperAdmin = isSuperAdmin;
    }
}

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem,
        int? branchId = null,
        int? userId = null,
        bool isSuperAdmin = false);

    ValueTask QueueBackgroundWorkItemAsync(BackgroundWorkItem item);

    ValueTask<BackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken);
}
