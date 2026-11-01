using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Service_API.Services;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<BackgroundWorkItem> _queue;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public BackgroundTaskQueue(IHttpContextAccessor? httpContextAccessor = null, int capacity = 1000)
    {
        _httpContextAccessor = httpContextAccessor;
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

        // Auto-capture caller's ambient tenant context if not explicitly provided
        if (!branchId.HasValue && _httpContextAccessor?.HttpContext?.User is { } user)
        {
            var branchClaim = user.FindFirst("branch_id")?.Value ?? user.FindFirst("BranchId")?.Value;
            if (int.TryParse(branchClaim, out var bId) && bId > 0)
            {
                branchId = bId;
            }

            if (!userId.HasValue)
            {
                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (int.TryParse(idClaim, out var uId))
                {
                    userId = uId;
                }
            }

            isSuperAdmin = isSuperAdmin
                || user.IsInRole("SuperAdmin")
                || string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(user.FindFirst("role")?.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }

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
