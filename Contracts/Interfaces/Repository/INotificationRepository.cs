using Contracts.DTOs.Notification;
using Contracts.interfaces.Repository;

namespace Contracts.Interfaces.Repository;

public interface INotificationRepository : IRepositoryBase<Entities.Models.Tables.Notification, NotificationDto, NotificationCreateDto, NotificationUpdateDto>
{
    Task<List<NotificationDto>> GetNotificationsForUserAsync(int userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}
