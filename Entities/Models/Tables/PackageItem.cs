using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class PackageItem : TenantBaseTable
{
    public int PackageId { get; set; }
    public Package? Package { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? TrackedProductId { get; set; }
    public Product? TrackedProduct { get; set; }

    public int? UntrackedItemId { get; set; }
    public UntrackedItem? UntrackedItem { get; set; }

    public int Quantity { get; set; } = 1;
}

