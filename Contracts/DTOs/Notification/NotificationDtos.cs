using Contracts.BaseDtos;
using Entities.Models.Enums;

namespace Contracts.DTOs.Notification;

public class NotificationDto : BaseDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public DateTime? InsertDate { get; set; }
}

public class NotificationCreateDto : BaseCreateDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}

public class NotificationUpdateDto : BaseUpdateDto
{
    public bool IsRead { get; set; }
}
