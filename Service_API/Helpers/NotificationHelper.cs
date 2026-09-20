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
        // 1. Find user groups assigned to this approval step
        var targetGroupIds = await context.ApprovalConfigs
            .Where(c => c.IsActive && c.RequestType == requestType && c.WorkflowRole == role)
            .Select(c => c.UserGroupId)
            .Distinct()
            .ToListAsync();

        // 2. Find all active users belonging to these groups or system Admins/SuperAdmins
        var targetUsers = await context.Users
            .Where(u => !u.IsDeleted && (
                (u.UserGroupId != null && targetGroupIds.Contains(u.UserGroupId.Value)) ||
                u.Role == UserRole.Admin ||
                u.Role == UserRole.SuperAdmin
            ))
            .Select(u => u.MilitaryNumber)
            .Distinct()
            .ToListAsync();

        // 3. Send notification to each user
        foreach (var userMilitaryNumber in targetUsers)
        {
            await repositoryWrapper.Notifications.Create(new NotificationCreateDto
            {
                UserId = userMilitaryNumber,
                Title = title,
                Message = message,
                Type = requestType == RequestType.Entry ? NotificationType.EntryRequest : NotificationType.ExitRequest,
                ReferenceId = referenceId,
                ReferenceType = referenceType
            });
        }
    }
}
