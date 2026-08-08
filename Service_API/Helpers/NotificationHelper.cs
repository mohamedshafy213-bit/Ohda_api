using Contracts.DTOs.Notification;
using Contracts.interfaces.Repository;
using Entities.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Service_API.Helpers;

public static class NotificationHelper
{
    public static async Task SendNotificationToUserAsync(
        IRepositoryWrapper repositoryWrapper,
        int userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        int? referenceId = null,
        string? referenceType = null)
    {
        await repositoryWrapper.Notifications.Create(new NotificationCreateDto
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = referenceId,
            ReferenceType = referenceType
        });
    }

    public static async Task SendNotificationToRoleAsync(
        IRepositoryWrapper repositoryWrapper,
        UserRole role,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        int? referenceId = null,
        string? referenceType = null)
    {
        var users = await repositoryWrapper.Users.FindAll();

        // Get users with matching role from repository or database
        var targetUsers = await repositoryWrapper.Users.FindAll();
        
        // Use direct EF Query if needed or query Users list
        var roleUsers = await repositoryWrapper.Users.FindAll();
        // Send to all users who have the role
    }
}
