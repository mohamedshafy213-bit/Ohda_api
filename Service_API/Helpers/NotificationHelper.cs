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
        if (userId <= 0) return;

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

    public static async Task NotifyWorkflowStepUsersAsync(
        IRepositoryWrapper repositoryWrapper,
        Entities.Models.Databases.RepositoryContext context,
        RequestType requestType,
        WorkflowRole role,
        string title,
        string message,
        int? referenceId = null,
        string? referenceType = null)
    {
        try
        {
            // 1. Find user groups assigned to this approval step
            var targetGroupIds = await context.ApprovalConfigs
                .AsNoTracking()
                .Where(c => c.IsActive && c.RequestType == requestType && c.WorkflowRole == role && !c.IsDeleted)
                .Select(c => c.UserGroupId)
                .Distinct()
                .ToListAsync();

            // 2. Find all active users belonging to these groups or system Admins/SuperAdmins
            var targetUserIds = await context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && (
                    (u.UserGroupId != null && targetGroupIds.Contains(u.UserGroupId.Value)) ||
                    u.Role == UserRole.Admin ||
                    u.Role == UserRole.SuperAdmin
                ))
                .Select(u => u.MilitaryNumber)
                .Distinct()
                .ToListAsync();

            // 3. Send notifications in a single batch
            if (targetUserIds.Any())
            {
                var notifications = targetUserIds.Select(userId => new Entities.Models.Tables.Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = requestType == RequestType.Entry ? NotificationType.EntryRequest : NotificationType.ExitRequest,
                    ReferenceId = referenceId,
                    ReferenceType = referenceType,
                    IsRead = false,
                    InsertDate = DateTime.UtcNow,
                    IsDeleted = false
                }).ToList();

                await context.Notifications.AddRangeAsync(notifications);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Log without failing parent workflow operation
            Console.WriteLine($"[NotificationHelper] Warning: Failed to broadcast notifications: {ex.Message}");
        }
    }
}
