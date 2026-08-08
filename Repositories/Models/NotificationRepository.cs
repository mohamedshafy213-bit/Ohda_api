using Contracts.DTOs.Notification;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repositories;

namespace Repositories.Models;

public class NotificationRepository
    : RepositoryBase<Notification, NotificationDto, NotificationCreateDto, NotificationUpdateDto>, INotificationRepository
{
    public NotificationRepository(
        ILoggerManager logger,
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        : base(logger, repositoryContext, httpContextAccessor, mapper)
    {
    }

    public async Task<List<NotificationDto>> GetNotificationsForUserAsync(int userId, bool unreadOnly = false)
    {
        var query = RepositoryContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.InsertDate)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                Type = n.Type,
                ReferenceId = n.ReferenceId,
                ReferenceType = n.ReferenceType,
                InsertDate = n.InsertDate
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await RepositoryContext.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await RepositoryContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.LastUpdate = DateTime.UtcNow;
            await RepositoryContext.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var unread = await RepositoryContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.LastUpdate = DateTime.UtcNow;
        }

        await RepositoryContext.SaveChangesAsync();
    }
}
