using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class PlatformAuditLog : BaseTable
{
    public int? UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = string.Empty;

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
}
