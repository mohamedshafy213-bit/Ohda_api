using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class Notification : BaseTable
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public NotificationType Type { get; set; } = NotificationType.Info;

    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}
