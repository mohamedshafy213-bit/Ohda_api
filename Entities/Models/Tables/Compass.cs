using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Compass : BaseTable
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty; // Location / Department
    public DateTime ExitDate { get; set; } = DateTime.UtcNow;
    public int? ProductExitRequestId { get; set; }
    public ProductExitRequest? ProductExitRequest { get; set; }
    public string? Notes { get; set; }
}
