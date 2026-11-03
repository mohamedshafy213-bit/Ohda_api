using Contracts.DTOs.Notification;
using Contracts.interfaces.Repository;

namespace Contracts.Interfaces.Repository;

public interface INotificationRepository : IRepositoryBase<Entities.Models.Tables.Notification, NotificationDto, NotificationCreateDto, NotificationUpdateDto>
{
    Task<List<NotificationDto>> GetNotificationsForUserAsync(long userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(long userId);
    Task MarkAsReadAsync(int notificationId, long userId);
    Task MarkAllAsReadAsync(long userId);
}
