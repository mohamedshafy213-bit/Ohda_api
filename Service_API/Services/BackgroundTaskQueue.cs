using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Service_API.Services;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<BackgroundWorkItem> _queue;

    public BackgroundTaskQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<BackgroundWorkItem>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem,
        int? branchId = null,
        int? userId = null,
        bool isSuperAdmin = false)
    {
        if (workItem == null)
            throw new ArgumentNullException(nameof(workItem));

        var item = new BackgroundWorkItem(workItem, branchId, userId, isSuperAdmin);
        await _queue.Writer.WriteAsync(item);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(BackgroundWorkItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        await _queue.Writer.WriteAsync(item);
    }

    public async ValueTask<BackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
