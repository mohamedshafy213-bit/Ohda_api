using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class Compass : BaseTable
{
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty; // Location / Department
    public DateTime ExitDate { get; set; } = DateTime.UtcNow;
    
    public CompassType Type { get; set; } = CompassType.Exit;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? ProductStateId { get; set; }
    public ProductState? ProductState { get; set; }

    public int? ProductExitRequestId { get; set; }
    public ProductExitRequest? ProductExitRequest { get; set; }

    public int? ProductEntryRequestId { get; set; }
    public ProductEntryRequest? ProductEntryRequest { get; set; }

    public string? Notes { get; set; }
}
