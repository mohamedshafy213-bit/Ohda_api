using Entities.Models.BaseTables;
using Entities.Models.Enums;

namespace Entities.Models.Tables;

public class ProductExitRequestItem : TenantBaseTable
{
    public int ProductExitRequestId { get; set; }
    public ProductExitRequest? ProductExitRequest { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public bool IsUntracked { get; set; } = false;
    public string? UntrackedItemName { get; set; }

    public int? PackageId { get; set; }
    public string? PackageName { get; set; }

    public int Quantity { get; set; } = 1;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? Notes { get; set; }
}
